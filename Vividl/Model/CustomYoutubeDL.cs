using System;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace Vividl.Model
{
    public class CustomYoutubeDL : YoutubeDL
    {
        public CustomYoutubeDL(byte maxNumberOfProcesses) : base(maxNumberOfProcesses)
        { }

        public string DownloadArchive { get; set; }

        public bool AddMetadata { get; set; }

        public string Proxy { get; set; }

        // Dumb way to determine if we are likely using yt-dlp
        public bool UsingYtDlp => YoutubeDLPath.Contains("yt-dlp");

        protected override OptionSet GetDownloadOptions()
        {
            var options = base.GetDownloadOptions();
            // Workaround to suppress the warning in yt-dlp
            if (UsingYtDlp)
            {
                options.ExternalDownloaderArgs = "ffmpeg:" + options.ExternalDownloaderArgs;
            }
            options.DownloadArchive = this.DownloadArchive;
            options.AddMetadata = this.AddMetadata;
            options.Proxy = this.Proxy;
            return options;
        }
    }
}
