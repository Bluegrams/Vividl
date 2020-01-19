using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Vividl.Services;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace Vividl.Model
{
    public class VideoEntry : MediaEntry
    {
        public override int TotalItems => 1;

        public string DownloadPath { get; private set; } = String.Empty;

        public VideoEntry(YoutubeDL ydl, VideoData metadata)
            : base(ydl, metadata)
        { }

        protected override async Task<DownloadResult> DoDownload(DownloadOption downloadOption)
        {
            try
            {
                var run = await downloadOption.RunDownload(ydl, this, cts.Token, progress);
                DownloadPath = run.Data;
                if (!run.Success) return DownloadResult.Failed;
            }
            catch (Exception ex)
            {
                // TODO Clean up partially downloaded files?
                DownloadPath = String.Empty;
                if (ex is TaskCanceledException) return DownloadResult.Cancelled;
                else return DownloadResult.Failed;
            }
            Debug.WriteLine($"Finished downloading to: \"{DownloadPath}\"");
            return DownloadResult.Success;
        }

        public override void OpenFile()
        {
            if (!String.IsNullOrEmpty(DownloadPath))
                Process.Start(DownloadPath);
        }

        public override void ShowInFolder(IFileService fileService)
        {
            fileService.ShowInExplorer(DownloadPath);
        }
    }
}
