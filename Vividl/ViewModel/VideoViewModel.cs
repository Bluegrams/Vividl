using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Messaging;
using Vividl.Model;
using Vividl.Properties;
using YoutubeDLSharp;

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

        public VideoViewModel(string url, MainViewModel<MediaEntry> mainVm)
            : base(url, mainVm) { }

        public async override Task Fetch(bool refetch = false)
        {
            try
            {
                Entry = await MediaEntry.Fetch(ToString());
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
            // Add additional download options
            if (Entry.Metadata.Formats != null)
            {
                this.availableFormats = Entry.Metadata.Formats.Select(fm =>
                {
                    return new VideoDownload(fm.FormatId,
                                    description: String.Format("{0} - {1}", fm.Extension, fm.Format),
                                    fileExtension: fm.Extension);
                });
                UpdateCurrentFormats();
            }
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
