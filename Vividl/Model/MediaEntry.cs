using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Ioc;
using Vividl.Properties;
using Vividl.Services;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

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
        public OptionSet OverrideOptions { get; }

        public abstract int TotalItems { get; }
        public abstract bool FileAvailable { get; }

        public IList<IDownloadOption> DownloadOptions { get; }
        public int SelectedDownloadOption { get; set; }

        public event EventHandler<ProgressEventArgs> DownloadStateChanged;

        public MediaEntry(YoutubeDL ydl, VideoData metadata, OptionSet overrideOptions = null)
        {
            this.ydl = ydl;
            this.Metadata = metadata;
            this.OverrideOptions = overrideOptions;
            this.DownloadOptions = new List<IDownloadOption>();
            this.progress = new Progress<DownloadProgress>(p => RaiseDownloadStateChanged(p));
        }

        public static async Task<MediaEntry> Fetch(string url, OptionSet overrideOptions = null)
        {
            var ytdl = SimpleIoc.Default.GetInstance<YoutubeDL>();
            var run = await ytdl.RunVideoDataFetch(url, overrideOptions: overrideOptions);
            if (!run.Success)
                throw new VideoEntryException(run.ErrorOutput);
            var metadata = run.Data;
            switch (metadata.ResultType)
            {
                case MetadataType.Playlist:
                case MetadataType.MultiVideo:
                    return new PlaylistEntry(ytdl, metadata, overrideOptions: overrideOptions);
                default:
                    return new VideoEntry(ytdl, metadata, overrideOptions: overrideOptions);
            }
        }

        protected abstract Task<DownloadResult> DoDownload(DownloadOption downloadOption);

        public async Task<DownloadResult> Download()
        {
            CancelDownload(); // Cancel ongoing download if existent
            cts = new CancellationTokenSource();
            Directory.CreateDirectory(Settings.Default.DownloadFolder);
            var result = await DoDownload((DownloadOption)DownloadOptions[SelectedDownloadOption]);
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
