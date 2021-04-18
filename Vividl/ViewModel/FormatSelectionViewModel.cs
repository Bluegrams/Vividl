using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Vividl.Model;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace Vividl.ViewModel
{
    public class FormatSelectionViewModel : ViewModelBase
    {
        // TODO v.0.5: Open FormatSelectionWindow when custom option is selected in combo box
        // TODO v.0.5: Add row with format selection download info (e.g. size)

        private readonly MediaEntry entry;
        private FormatData selectedAudioVideo, selectedAudio, selectedVideo;
        private bool hasAudioExtraction;
        private VideoRecodeFormat videoRecodeFormat;
        private AudioConversionFormat audioConversionFormat;

        public IList<IDownloadOption> DownloadOptions
            => entry.DownloadOptions;

        public int SelectedDownloadOption
        {
            get => entry.SelectedDownloadOption;
            set
            {
                entry.SelectedDownloadOption = value;
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

        public ICommand ApplyFormatSelectionCommand { get; }

        public ICommand DownloadCommand { get; }

        public FormatSelectionViewModel(VideoViewModel videoVm)
        {
            this.entry = videoVm.Entry;
            applySelectedDownloadOption();
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
            var selectedOption = DownloadOptions[SelectedDownloadOption];
            // We only need to do something if custom options are selected
            if (selectedOption is CustomDownload customDownloadOption)
            {
                // TODO v.0.5: Prevent user selection of invalid conversion formats
                customDownloadOption.Configure(
                    SelectedVideo ?? SelectedAudioVideo, SelectedAudio,
                    HasAudioExtraction, AudioConversionFormat, VideoRecodeFormat
                );
            }
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
        }

        private void applySelectedDownloadOption()
        {
            var selectedOption = DownloadOptions[SelectedDownloadOption];
            if (selectedOption is CustomDownload customDownloadOption)
            {
                // VideoFormat of custom download option can be either video only or video+audio
                if (AudioVideoDownloadOptions.Contains(customDownloadOption.VideoFormat))
                {
                    this.selectedAudioVideo = customDownloadOption.VideoFormat;
                }
                else
                {
                    this.selectedVideo = customDownloadOption.VideoFormat;
                }
                this.selectedAudio = customDownloadOption.AudioFormat;
                this.hasAudioExtraction = customDownloadOption.IsAudio;
                this.audioConversionFormat = customDownloadOption.AudioConversionFormat;
                this.videoRecodeFormat = customDownloadOption.VideoRecodeFormat;
            }
            else
            {
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
            }
            this.CurrentPage = this.selectedAudio != null || this.selectedVideo != null ? 1 : 0;
            signalChangedProperties();
        }

        private void applyFormatSelectionChange(
            FormatData selectedAudioVideo = null, FormatData selectedVideo = null, FormatData selectedAudio = null)
        {
            // select custom download
            entry.SelectedDownloadOption = DownloadOptions.IndexOf(DownloadOptions.FirstOrDefault(f => f is CustomDownload));
            this.selectedAudioVideo = selectedAudioVideo;
            this.selectedVideo = selectedVideo;
            this.selectedAudio = selectedAudio;
            signalChangedProperties();
        }

        private void applyConversionChange()
        {
            // select custom download
            entry.SelectedDownloadOption = DownloadOptions.IndexOf(DownloadOptions.FirstOrDefault(f => f is CustomDownload));
            signalChangedProperties();
        }
    }
}
