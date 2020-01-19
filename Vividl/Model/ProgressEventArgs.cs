using System;
using YoutubeDLSharp;

namespace Vividl.Model
{
    public class ProgressEventArgs : EventArgs
    {
        public DownloadProgress Info { get; }

        public ProgressEventArgs(DownloadProgress info)
        {
            this.Info = info;
        }
    }
}
