using System;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace Vividl.Model
{
    class CustomYoutubeDL : YoutubeDL
    {
        public CustomYoutubeDL(byte maxNumberOfProcesses) : base(maxNumberOfProcesses)
        { }

        public string DownloadArchive { get; set; }

        protected override OptionSet GetDownloadOptions()
        {
            var options = base.GetDownloadOptions();
            options.DownloadArchive = this.DownloadArchive;
            return options;
        }
    }
}
