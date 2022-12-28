using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Vividl.Model;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;
using ByteSizeLib;
using Vividl.Properties;

namespace Vividl.ViewModel
{
    public class FormatSelectionViewModel : ViewModelBase
    {
        private readonly MediaEntry entry;
        private int selectedDownloadOption;
        private FormatData selectedAudioVideo, selectedAudio, selectedVideo;
        private bool hasAudioExtraction;
        private VideoRecodeFormat videoRecodeFormat;
        private AudioConversionFormat audioConversionFormat = AudioConversionFormat.Mp3;
        // Tracks changes on custom download configuration
        private CustomDownload customDownload;
        private bool isSelectionValid = true;
        private string selectionErrorMessage;

        public DownloadOptionCollection DownloadOptions
            => entry.DownloadOptions;

        // This property should be persisted to `entry.SelectedDownloadOption`.
        public int SelectedDownloadOption
        {
            get => selectedDownloadOption;
            set
            {
                selectedDownloadOption = value;
                applySelectedDownloadOption();
            }
        }

        public IEnumerable<FormatData> AudioVideoDownloadOptions
            => this.entry.Metadata.GetAudioVideoFormats();

        public IEnumerable<FormatData> AudioOnlyDownloadOptions
            => this.entry.Metadata.GetAudioOnlyFormats();

        public IEnumerable<FormatData> VideoOnlyDownloadOptions
            => this.entry.Metadata.GetVideoOnlyFormats();

        public FormatData SelectedAudioVideo
        {
            get => selectedAudioVideo;
            set
            {
                applyFormatSelectionChange(selectedAudioVideo: value);
            }
        }

        public FormatData SelectedVideo
        {
            get => selectedVideo;
            set
            {
                applyFormatSelectionChange(selectedVideo: value, selectedAudio: SelectedAudio);
            }
        }

        public FormatData SelectedAudio
        {
            get => selectedAudio;
            set
            {
                applyFormatSelectionChange(selectedVideo: SelectedVideo, selectedAudio: value);
            }
        }

        public bool HasSelectedVideo
        {
            get => selectedVideo != null;
            set
            {
                if (value)
                {
                    applyFormatSelectionChange(
                        selectedVideo: VideoOnlyDownloadOptions.First(), selectedAudio: SelectedAudio
                    );
                }
                else applyFormatSelectionChange(
                    selectedVideo: null, selectedAudio: SelectedAudio
                );
            }
        }

        public bool HasSelectedAudio
        {
            get => selectedAudio != null;
            set
            {
                if (value)
                {
                    applyFormatSelectionChange(
                        selectedVideo: SelectedVideo, selectedAudio: AudioOnlyDownloadOptions.First()
                    );
                }
                else applyFormatSelectionChange(
                    selectedVideo: SelectedVideo, selectedAudio: null
                );
            }
        }

        public int CurrentPage { get; set; }

        public bool HasAudioExtraction
        {
            get => hasAudioExtraction;
            set
            {
                this.hasAudioExtraction = value;
                applyConversionChange();
            }
        }

        public VideoRecodeFormat VideoRecodeFormat
        {
            get => videoRecodeFormat;
            set
            {
                this.videoRecodeFormat = value;
                applyConversionChange();
            }
        }

        public AudioConversionFormat AudioConversionFormat
        {
            get => audioConversionFormat;
            set
            {
                this.audioConversionFormat = value;
                applyConversionChange();
            }
        }

        public string FileExtension
        {
            get
            {
                // use the cached changes if custom download is selected
                if (SelectedDownloadOption == DownloadOptions.CustomDownloadIndex)
                    return customDownload.GetExt();
                else return DownloadOptions[SelectedDownloadOption].GetExt(
                    // if not able to resolve, we have "best" option without recoding
                    defaultValue: SelectedAudioVideo?.Extension
                );
            }
        }

        public string VideoWidthAndHeight => SelectedVideo?.GetWidthAndHeight() ?? SelectedAudioVideo?.GetWidthAndHeight();

        public string DownloadSize
        {
            get
            {
                long bytes = 0L;
                foreach (var format in new[] { SelectedAudioVideo, SelectedVideo, SelectedAudio})
                {
                    bytes += format?.FileSize ?? format?.ApproximateFileSize ?? 0L;
                }
                return ByteSize.FromBytes(bytes).ToString();
            }
        }

        public bool IsSelectionValid
        {
            get => isSelectionValid;
            set
            {
                isSelectionValid = value;
                RaisePropertyChanged();
            }
        }

        public string SelectionErrorMessage
        {
            get => selectionErrorMessage;
            set
            {
                selectionErrorMessage = value;
                RaisePropertyChanged();
            }
        }

        public ICommand ApplyFormatSelectionCommand { get; }

        public ICommand DownloadCommand { get; }

        public FormatSelectionViewModel(VideoViewModel videoVm)
        {
            this.entry = videoVm.Entry;
            this.customDownload = this.entry.DownloadOptions.CustomDownload;
            this.SelectedDownloadOption = this.entry.SelectedDownloadOption;
            // Init commands
            ApplyFormatSelectionCommand = new RelayCommand(
                () => ApplyFormatSelection(), () => canApplyFormatSelection(), keepTargetAlive: true
            );
            DownloadCommand = new RelayCommand(async () =>
                {
                    ApplyFormatSelection();
                    await videoVm.DownloadVideo();
                },
                () => canApplyFormatSelection(), keepTargetAlive: true
            );
        }

        private bool canApplyFormatSelection()
            => SelectedAudioVideo != null || SelectedVideo != null || SelectedAudio != null;

        public void ApplyFormatSelection()
        {
            // We only need to do something if custom options are selected
            if (SelectedDownloadOption == DownloadOptions.CustomDownloadIndex)
            {
                DownloadOptions.CustomDownload = customDownload;
            }
            // Persist SelectedDownloadOption
            entry.SelectedDownloadOption = SelectedDownloadOption;
        }

        private void signalChangedProperties()
        {
            // List properties for which to signal changes. Using "null" would cause a circle.
            RaisePropertyChanged(nameof(SelectedDownloadOption));
            RaisePropertyChanged(nameof(SelectedAudioVideo));
            RaisePropertyChanged(nameof(SelectedVideo));
            RaisePropertyChanged(nameof(SelectedAudio));
            RaisePropertyChanged(nameof(HasSelectedVideo));
            RaisePropertyChanged(nameof(HasSelectedAudio));
            RaisePropertyChanged(nameof(CurrentPage));
            RaisePropertyChanged(nameof(HasAudioExtraction));
            RaisePropertyChanged(nameof(VideoRecodeFormat));
            RaisePropertyChanged(nameof(AudioConversionFormat));
            RaisePropertyChanged(nameof(FileExtension));
            RaisePropertyChanged(nameof(VideoWidthAndHeight));
            RaisePropertyChanged(nameof(DownloadSize));
        }

        private void applySelectedDownloadOption()
        {
            // Check for type in DownloadOptions list but use the cached custom download object.
            if (SelectedDownloadOption == DownloadOptions.CustomDownloadIndex)
            {
                // VideoFormat of custom download option can be either video only or video+audio
                if (AudioVideoDownloadOptions.Contains(customDownload.VideoFormat))
                {
                    this.selectedAudioVideo = customDownload.VideoFormat;
                }
                else
                {
                    this.selectedVideo = customDownload.VideoFormat;
                }
                this.selectedAudio = customDownload.AudioFormat;
                this.hasAudioExtraction = customDownload.IsAudio;
                this.audioConversionFormat = customDownload.AudioConversionFormat;
                this.videoRecodeFormat = customDownload.VideoRecodeFormat;
            }
            else
            {
                var selectedOption = DownloadOptions[SelectedDownloadOption];
                FormatData[] formats = entry.Metadata.SelectFormat(selectedOption.FormatSelection);
                if (formats != null)
                {
                    this.selectedAudioVideo = formats.FirstOrDefault(f => f.VideoCodec != "none" && f.AudioCodec != "none");
                    this.selectedAudio = formats.FirstOrDefault(f => f.VideoCodec == "none");
                    this.selectedVideo = formats.FirstOrDefault(f => f.AudioCodec == "none");
                }
                this.hasAudioExtraction = selectedOption is AudioConversionDownload;
                if (hasAudioExtraction)
                {
                    this.audioConversionFormat = ((AudioConversionDownload)selectedOption).ConversionFormat;
                }
                else
                {
                    this.videoRecodeFormat = ((VideoDownload)selectedOption).RecodeFormat;
                }
                // selecting a pre-defined option is always valid
                IsSelectionValid = true;
                SelectionErrorMessage = null;
            }
            // Avoid AudioConversionFormat.Best
            if (this.audioConversionFormat == AudioConversionFormat.Best)
            {
                this.audioConversionFormat = AudioConversionFormat.Mp3;
            }
            this.CurrentPage = this.selectedAudio != null || this.selectedVideo != null ? 1 : 0;
            signalChangedProperties();
        }

        private void applyFormatSelectionChange(
            FormatData selectedAudioVideo = null, FormatData selectedVideo = null, FormatData selectedAudio = null)
        {
            this.selectedAudioVideo = selectedAudioVideo;
            this.selectedVideo = selectedVideo;
            this.selectedAudio = selectedAudio;
            tryUpdateCustomDownload();
            signalChangedProperties();
        }

        private void applyConversionChange()
        {
            tryUpdateCustomDownload();
            signalChangedProperties();
        }

        private void tryUpdateCustomDownload()
        {
            // Check if we have selected one of the default formats w/o any conversion:
            // This is the case if conversions are deactivated & exactly one format is selected
            if (!HasAudioExtraction && VideoRecodeFormat == VideoRecodeFormat.None
                && (SelectedAudioVideo != null ^ SelectedVideo != null ^ SelectedAudio != null))
            {
                FormatData format = SelectedAudioVideo ?? SelectedVideo ?? SelectedAudio;
                int index = DownloadOptions.IndexOfFirstOrDefault(d => d.FormatSelection == format?.FormatId);
                if (index >= 0)
                {
                    this.selectedDownloadOption = index;
                    return;
                }
            }

            this.selectedDownloadOption = DownloadOptions.CustomDownloadIndex;
            var newCustomDownload = new CustomDownload(Resources.DownloadOption_Custom);
            try
            {
                newCustomDownload.Configure(
                    SelectedVideo ?? SelectedAudioVideo, SelectedAudio,
                    HasAudioExtraction, AudioConversionFormat, VideoRecodeFormat
                );
            }
            catch (Exception ex)
            {
                IsSelectionValid = false;
                SelectionErrorMessage = ex.Message;
                return;
            }
            customDownload = newCustomDownload;
            IsSelectionValid = true;
            SelectionErrorMessage = null;
        }
    }
}
