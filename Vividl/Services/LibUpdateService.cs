using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Bluegrams.Application;
using Vividl.Properties;
using YoutubeDLSharp;

namespace Vividl.Services
{
    public interface ILibUpdateService
    {
        string Version { get; }
        bool IsUpdating { get; }
        Task<bool> CheckForUpdates();
        Task<string> Update();
    }

    public class YtdlUpdateService : ILibUpdateService, INotifyPropertyChanged
    {
        public const string YTDL_LATEST_VERSION_URL = "https://yt-dl.org/update/LATEST_VERSION";

        private YoutubeDL ydl;
        private IDialogService dialogService;
        private bool isUpdating;

        public YtdlUpdateService(YoutubeDL ydl, IDialogService dialogService)
        {
            this.ydl = ydl;
            this.dialogService = dialogService;
        }

        public string Version => ydl.Version;

        public bool IsUpdating
        {
            get => isUpdating;
            set
            {
                isUpdating = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUpdating)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task<bool> CheckForUpdates()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    string latestVersion = await client.DownloadStringTaskAsync(YTDL_LATEST_VERSION_URL);
                    if (new Version(latestVersion) > new Version(this.Version))
                    {
                        return dialogService.ShowConfirmation(
                            String.Format(Resources.YtdlUpdateService_NewUpdateMessage, latestVersion, this.Version),
                            "Vividl - " + Resources.Info
                        );
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public async Task<string> Update()
        {
            IsUpdating = true;
            var output = await ydl.RunUpdate();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Version)));
            IsUpdating = false;
            return output;
        }
    }
}
