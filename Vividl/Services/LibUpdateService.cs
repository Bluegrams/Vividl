using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Bluegrams.Application;
using Newtonsoft.Json;
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

        protected YoutubeDL ydl;
        protected IDialogService dialogService;
        protected bool isUpdating;

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

        public virtual async Task<bool> CheckForUpdates()
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

    /// <summary>
    /// Service class that checks for updates to yt-dlp on its project website.
    /// </summary>
    public class YtDlpUpdateService : YtdlUpdateService
    {
        public new const string YTDL_LATEST_VERSION_URL = "https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest";

        public YtDlpUpdateService(YoutubeDL ydl, IDialogService dialogService) : base(ydl, dialogService) { }

        public override async Task<bool> CheckForUpdates()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    string jsonString = await client.DownloadStringTaskAsync(YTDL_LATEST_VERSION_URL);
                    Dictionary<string, string> versionInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
                    string latestVersion = versionInfo["tag_name"];
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
    }
}
