using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Vividl.Services;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace Vividl.Model
{
    public class PlaylistEntry : MediaEntry
    {
        public override int TotalItems => Metadata.Entries?.Length ?? 1;
        public override bool FileAvailable
            => DownloadPaths != null && DownloadPaths.Length > 0 && !String.IsNullOrEmpty(DownloadPaths[0]);

        public string[] DownloadPaths { get; private set; }

        public PlaylistEntry(YoutubeDL ydl, VideoData metadata)
            : base(ydl, metadata)
        { }
        
        protected override async Task<DownloadResult> DoDownload(DownloadOption downloadOption)
        {
            try
            {
                var run = await downloadOption.RunDownload(ydl, this, cts.Token, progress);
                DownloadPaths = run.Data;
                // TODO When does playlist download count as 'failed'?
                if (!run.Success) return DownloadResult.Failed;
            }
            catch (Exception ex)
            {
                // TODO Clean up partially downloaded files?
                DownloadPaths = null;
                if (ex is TaskCanceledException) return DownloadResult.Cancelled;
                else return DownloadResult.Failed;
            }
            return DownloadResult.Success;
        }

        public override void OpenFile()
        {
            if (FileAvailable)
                Process.Start(DownloadPaths[0]);
        }

        public override void ShowInFolder(IFileService fileService)
        {
            if (FileAvailable)
                fileService.ShowInExplorer(DownloadPaths);
        }
    }
}
