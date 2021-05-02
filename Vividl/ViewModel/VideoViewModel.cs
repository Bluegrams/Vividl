using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Vividl.Model;
using Vividl.Properties;
using Vividl.Services;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace Vividl.ViewModel
{
    public class VideoViewModel : ItemViewModel<MediaEntry>
    {
        public override bool IsPlaylist => Entry is PlaylistEntry;

        public string Duration
            => TimeSpan.FromSeconds(Entry.Metadata.Duration.GetValueOrDefault()).ToString("c");

        public override string InformationString
        {
            get
            {
                if (Entry is PlaylistEntry)
                {
                    int count = Entry.Metadata.Entries.Length;
                    return String.Format(Resources.VideoEntry_Playlist, count);
                }
                else return this.Duration;
            }
        }
        public ICommand CustomizeDownloadCommand { get; }

        public VideoViewModel(string url, MainViewModel<MediaEntry> mainVm)
            : base(url, mainVm)
        {
            CustomizeDownloadCommand = new RelayCommand<Action<bool>>(
                (callback) => Messenger.Default.Send(new ShowWindowMessage(
                    WindowType.FormatSelectionWindow,
                    new FormatSelectionViewModel(this),
                    (r, o) => 
                    {
                        RaisePropertyChanged(nameof(SelectedDownloadOption));
                        callback?.Invoke(r.GetValueOrDefault());
                    }
                )),
                (callback) => State == ItemState.Fetched
            );
        }

        public async override Task Fetch(bool refetch = false, OptionSet overrideOptions = null)
        {
            try
            {
                Entry = await MediaEntry.Fetch(ToString(), overrideOptions: overrideOptions);
            }
            catch (Exception ex)
            {
                Unavailable = true;
                if (ex is VideoEntryException vidEx)
                    messageService.ShowError(vidEx.FirstSentence, Resources.MainWindow_FetchFailed_Title);
                else messageService.ShowError(ex.Message, Resources.Error);
                return;
            }
            Entry.DownloadStateChanged += videoDownloadStateChanged;
            State = ItemState.Fetched;
            initializeDownloadOptions();
        }

        private void initializeDownloadOptions()
        {
            // Add the default download options
            var downloadOptionProvider = SimpleIoc.Default.GetInstance<IDownloadOptionProvider>();
            foreach (var option in downloadOptionProvider.CreateDownloadOptions(!IsPlaylist))
            {
                Entry.DownloadOptions.Add(option);
            }
            defaultOptionsCount = Entry.DownloadOptions.Count;
            SelectedDownloadOption = Settings.Default.DefaultFormat;
            // Add additional download options
            if (Entry.Metadata.Formats != null)
            {
                var metadataOptions = Entry.Metadata.Formats.Select(fm =>
                {
                    return new VideoDownload(fm.FormatId,
                                    description: String.Format("[{0}] {1}", fm.Extension, fm.Format),
                                    fileExtension: fm.Extension,
                                    isAudio: fm.VideoCodec == "none");
                });
                foreach (var option in metadataOptions)
                {
                    Entry.DownloadOptions.Add(option);
                }
            }
            RaisePropertyChanged(nameof(DownloadOptions));
        }

        private void videoDownloadStateChanged(object _, ProgressEventArgs e)
        {
            CurrentProgress = e.Info.Progress;
            switch (e.Info.State)
            {
                case DownloadState.PreProcessing:
                    ProgressString = Resources.VideoEntry_PreProcessing;
                    break;
                case DownloadState.Downloading:
                    ProgressString = Resources.VideoEntry_Downloading;
                    TotalDownloadSize = e.Info.TotalDownloadSize;
                    DownloadSpeed = e.Info.DownloadSpeed;
                    DownloadTimeRemaining = e.Info.ETA;
                    if (State == ItemState.Downloading) // Prevent msg being sent after cancelled msg.
                    {
                        Messenger.Default.Send(
                            new NotificationMessage(String.Format(Resources.Video_DownloadStarting, Entry.Title)));
                    }
                    break;
                case DownloadState.PostProcessing:
                    ProgressString = Resources.VideoEntry_Converting;
                    break;
                case DownloadState.Success:
                    incrDownloadIndex();
                    var name = Path.GetFileName(e.Info.Data);
                    Messenger.Default.Send(
                        new NotificationMessage(String.Format(Resources.Video_DownloadFinished, name)));
                    break;
                case DownloadState.Error:
                    messageService.ShowError(e.Info.Data, $"\"{Entry.Title}\"");
                    break;
            }
        }
    }
}
