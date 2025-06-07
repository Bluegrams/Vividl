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

        public string FormatSort { get; set; }

        public OptionSet CustomDownloadOptions { get; set; }

        protected override OptionSet GetDownloadOptions()
        {
            var options = base.GetDownloadOptions();
            options.Progress = true;
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
            options.FormatSort = this.FormatSort;
            if (this.CustomDownloadOptions != null)
            {
                options = options.OverrideOptions(this.CustomDownloadOptions);
            }
            return options;
        }
    }
}
