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

        public OptionSet CustomDownloadOptions { get; set; }

        // Dumb way to determine if we are likely using yt-dlp
        public bool UsingYtDlp => YoutubeDLPath.Contains("yt-dlp");

        protected override OptionSet GetDownloadOptions()
        {
            var options = base.GetDownloadOptions();
            #if LegacyYoutubeDLSharp
            // Workaround to suppress the warning in yt-dlp
            if (UsingYtDlp)
            {
                options.ExternalDownloaderArgs = "ffmpeg:" + options.ExternalDownloaderArgs;
            }
            #endif
            options.DownloadArchive = this.DownloadArchive;
            #if LegacyYoutubeDLSharp
            options.AddMetadata = this.AddMetadata;
            #else
            options.EmbedMetadata = this.AddMetadata;
            #endif
            options.Proxy = this.Proxy;
            if (this.CustomDownloadOptions != null)
            {
                options = options.OverrideOptions(this.CustomDownloadOptions);
            }
            return options;
        }
    }
}
