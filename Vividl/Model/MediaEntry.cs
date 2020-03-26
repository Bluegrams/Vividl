using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using Vividl.Properties;
using Vividl.Services;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;

namespace Vividl.Model
{
    public abstract class MediaEntry : IDownloadEntry
    {
        protected readonly YoutubeDL ydl;
        protected CancellationTokenSource cts;
        protected readonly IProgress<DownloadProgress> progress;

        public VideoData Metadata { get; }
        public string Url => Metadata.WebpageUrl;
        public string Title => Metadata.Title;
        public abstract int TotalItems { get; }
        public abstract bool FileAvailable { get; }

        public event EventHandler<ProgressEventArgs> DownloadStateChanged;

        public MediaEntry(YoutubeDL ydl, VideoData metadata)
        {
            this.ydl = ydl;
            this.Metadata = metadata;
            this.progress = new Progress<DownloadProgress>(p => RaiseDownloadStateChanged(p));
        }

        public static async Task<MediaEntry> Fetch(string url)
        {
            var ytdl = SimpleIoc.Default.GetInstance<YoutubeDL>();
            var run = await ytdl.RunVideoDataFetch(url);
            if (!run.Success)
                throw new VideoEntryException(run.ErrorOutput);
            var metadata = run.Data;
            switch (metadata.ResultType)
            {
                case MetadataType.Playlist:
                case MetadataType.MultiVideo:
                    return new PlaylistEntry(ytdl, metadata);
                default:
                    return new VideoEntry(ytdl, metadata);
            }
        }

        protected abstract Task<DownloadResult> DoDownload(DownloadOption downloadOption);

        public async Task<DownloadResult> DownloadVideo(IDownloadOption downloadOption)
        {
            CancelDownload(); // Cancel ongoing download if existent
            cts = new CancellationTokenSource();
            Directory.CreateDirectory(Settings.Default.DownloadFolder);
            var result = await DoDownload((DownloadOption)downloadOption);
            cts.Dispose();
            cts = null;
            return result;
        }

        public void CancelDownload()
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        public void OpenInBrowser() => Process.Start(Url);

        public abstract void OpenFile();

        public abstract void ShowInFolder(IFileService fileService);

        protected void RaiseDownloadStateChanged(DownloadProgress p)
            => DownloadStateChanged?.Invoke(this, new ProgressEventArgs(p));
    }
}
