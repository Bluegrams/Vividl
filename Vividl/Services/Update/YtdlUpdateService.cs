using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Bluegrams.Application;
using Newtonsoft.Json;
using Vividl.Model;
using Vividl.Properties;

namespace Vividl.Services.Update
{
    public class YtdlUpdateService : ILibUpdateService, INotifyPropertyChanged
    {
        public const string YTDL_LATEST_VERSION_URL = "https://yt-dl.org/update/LATEST_VERSION";
        public const string YTDLP_LATEST_VERSION_URL = "https://api.github.com/repos/yt-dlp/yt-dlp/releases/latest";

        private CustomYoutubeDL ydl;
        private IDialogService dialogService;
        private bool isUpdating;

        public YtdlUpdateService(CustomYoutubeDL ydl, IDialogService dialogService)
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
            if (App.UsingYtDlp)
            {
                return await checkForUpdatesYtDlp();
            }
            else
            {
                return await checkForUpdatesYoutubeDL();
            }
        }

        private async Task<bool> checkForUpdatesYoutubeDL()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    string latestVersion = await client.DownloadStringTaskAsync(YTDL_LATEST_VERSION_URL);
                    Debug.WriteLine("[youtube-dl update check] Found version: " + latestVersion);
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

        private async Task<bool> checkForUpdatesYtDlp()
        {
            IsUpdating = true;
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("User-Agent: Other");
                try
                {
                    string jsonString = await client.DownloadStringTaskAsync(YTDLP_LATEST_VERSION_URL);
                    var versionInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                    string latestVersion = (string)versionInfo["tag_name"];
                    Debug.WriteLine("[yt-dlp update check] Found version: " + latestVersion);
                    if (new Version(latestVersion) > new Version(this.Version))
                    {
                        string name = App.UsingYtDlp ? "yt-dlp" : "youtube-dl";
                        return dialogService.ShowConfirmation(
                            String.Format(Resources.YtdlUpdateService_NewUpdateMessage, latestVersion, this.Version, name),
                            "Vividl - " + Resources.Info
                        );
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    IsUpdating = false;
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
