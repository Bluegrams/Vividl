using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vividl.Services;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace Vividl.Model
{
    public abstract class DownloadOption : IDownloadOption
    {
        /// <summary>
        /// If this field is set to true, the download method checks the existence of the requested file before handing over to youtube-dl.
        /// This is required for all downloads including conversion as the pre-conversion file would be redownloaded otherwise.
        /// </summary>
        private readonly bool allowsOverwriteCheck;

        public abstract string FormatSelection { get; }

        public string Description { get; }

        public bool IsAudio { get; protected set; }

        public DownloadOption(string description, bool isAudio, bool allowsOverwriteCheck)
        {
            this.Description = description;
            this.IsAudio = isAudio;
            this.allowsOverwriteCheck = allowsOverwriteCheck;
        }

        /// <summary>
        /// Gets the final file extension of this download format.
        /// This reduces the flexibility of the download process as we cannot support
        /// generic 'best' downloads (see implementations). But this is needed to determine the
        /// final file path for pre-download checks.
        /// </summary>
        protected abstract string GetExt();

        /// <summary>
        /// Performs the actual download of the specified URL by invoking the given YoutubeDL instance.
        /// </summary>
        protected abstract Task<RunResult<string>> RunRealDownload(YoutubeDL ydl, string url,
            CancellationToken ct, IProgress<DownloadProgress> progress, OptionSet overrideOptions = null);

        protected abstract Task<RunResult<string[]>> RunRealPlaylistDownload(YoutubeDL ydl, string url,
            int[] playlistItems, CancellationToken ct, IProgress<DownloadProgress> progress, OptionSet overrideOptions = null);

        /// <summary>
        /// Performs the download of the specified video with the given YoutubeDL instance and a prior overwrite check.
        /// </summary>
        public async Task<RunResult<string>> RunDownload(YoutubeDL ydl, VideoEntry video,
            CancellationToken ct, IProgress<DownloadProgress> progress, OptionSet overrideOptions = null)
        {
            if (!ydl.OverwriteFiles && allowsOverwriteCheck)
            {
                bool restricted = ydl.RestrictFilenames;
                string ext = GetExt();
                string path = Path.Combine(ydl.OutputFolder, $"{Utils.Sanitize(video.Title, restricted)}.{ext}");
                if (File.Exists(path))
                {
                    // Don't redownload if file exists.
                    progress?.Report(new DownloadProgress(DownloadState.Success, data: path));
                    return new RunResult<string>(true, new string[0], path);
                }
            }
            return await RunRealDownload(ydl, video.Url, ct, progress, overrideOptions);
        }

        /// <summary>
        /// Performs the download of the specified playlist with the given YoutubeDL instance and a prior overwrite check.
        /// </summary>
        public async Task<RunResult<string[]>> RunDownload(YoutubeDL ydl, PlaylistEntry playlist,
            CancellationToken ct, IProgress<DownloadProgress> progress, OptionSet overrideOptions = null)
        {
            if (!ydl.OverwriteFiles && allowsOverwriteCheck)
            {
                bool restricted = ydl.RestrictFilenames;
                string ext = GetExt();
                // Check if some videos have already been downloaded.
                var indices = new List<int>();
                int index = 1;
                foreach (var video in playlist.Metadata.Entries)
                {
                    string path = Path.Combine(ydl.OutputFolder, $"{Utils.Sanitize(video.Title, restricted)}.{ext}");
                    System.Diagnostics.Debug.WriteLine(path);
                    if (!File.Exists(path)) indices.Add(index);
                    else progress?.Report(new DownloadProgress(DownloadState.Success, data: path));
                    index++;
                }
                return await RunRealPlaylistDownload(ydl, playlist.Url,
                    indices.ToArray(), ct, progress, overrideOptions);
            }
            else return await RunRealPlaylistDownload(ydl, playlist.Url, null, ct, progress, overrideOptions);
        }

        public override string ToString() => Description ?? base.ToString();
    }

    public class AudioConversionDownload : DownloadOption
    {
        public override string FormatSelection => "bestaudio/best";

        public AudioConversionFormat ConversionFormat { get; }

        public AudioConversionDownload(AudioConversionFormat format,
            string description = null)
            : base(description, true, true)
        {
            this.ConversionFormat = format;
        }

        protected override string GetExt() => ExtProvider.GetExtForAudio(ConversionFormat);

        protected override async Task<RunResult<string>> RunRealDownload(YoutubeDL ydl, string url,
            CancellationToken ct, IProgress<DownloadProgress> progress, OptionSet overrideOptions = null)
        {
            return await ydl.RunAudioDownload(
                url, ConversionFormat, ct, progress,
                output: new Progress<string>(s => DownloadOutputLogger.Instance.WriteOutput(url, s)),
                overrideOptions: overrideOptions
            );
        }

        protected override async Task<RunResult<string[]>> RunRealPlaylistDownload(YoutubeDL ydl, string url,
            int[] playlistItems, CancellationToken ct, IProgress<DownloadProgress> progress, OptionSet overrideOptions = null)
        {
            return await ydl.RunAudioPlaylistDownload(url, items: playlistItems,
                format: ConversionFormat, ct: ct, progress: progress,
                output: new Progress<string>(s => DownloadOutputLogger.Instance.WriteOutput(url, s)),
                overrideOptions: overrideOptions
            );
        }
    }

    public class VideoDownload : DownloadOption
    {
        public override string FormatSelection { get; }

        public VideoRecodeFormat RecodeFormat { get; }

        /* Use this to manually specify the file extension of this download option.
         * If this field is empty, we try to infer the file extension from the selected video format. */
        private string fileExtension;

        public VideoDownload(string formatSelection,
                            VideoRecodeFormat recodeFormat = VideoRecodeFormat.None,
                            string description = null, string fileExtension = "", bool isAudio = false)
            : base(description, isAudio, !String.IsNullOrEmpty(fileExtension) || recodeFormat != VideoRecodeFormat.None)
        {
            this.FormatSelection = formatSelection;
            this.RecodeFormat = recodeFormat;
            this.fileExtension = fileExtension;
        }

        protected override string GetExt()
        {
            if (!String.IsNullOrWhiteSpace(fileExtension))
                return fileExtension;
            return ExtProvider.GetExtForVideo(RecodeFormat);
        }

        protected override async Task<RunResult<string>> RunRealDownload(YoutubeDL ydl, string url,
            CancellationToken ct, IProgress<DownloadProgress> progress, OptionSet overrideOptions = null)
        {
            return await ydl.RunVideoDownload(
                url, FormatSelection,
                DownloadMergeFormat.Mkv, RecodeFormat, ct, progress,
                output: new Progress<string>(s => DownloadOutputLogger.Instance.WriteOutput(url, s)),
                overrideOptions: overrideOptions
            );
        }

        protected override async Task<RunResult<string[]>> RunRealPlaylistDownload(YoutubeDL ydl, string url,
            int[] playlistItems, CancellationToken ct, IProgress<DownloadProgress> progress, OptionSet overrideOptions = null)
        {
            return await ydl.RunVideoPlaylistDownload(url, format: FormatSelection,
                items: playlistItems, recodeFormat: RecodeFormat, ct: ct, progress: progress,
                output: new Progress<string>(s => DownloadOutputLogger.Instance.WriteOutput(url, s)),
                overrideOptions: overrideOptions
            );
        }
    }

    public class CustomDownload : DownloadOption
    {
        public FormatData AudioFormat { get; private set; }
        public FormatData VideoFormat { get; private set; }

        public override string FormatSelection
        {
            get
            {
                if (AudioFormat == null) return VideoFormat?.FormatId;
                else return VideoFormat.FormatId + "+" + AudioFormat.FormatId;
            }
        }

        public AudioConversionFormat AudioConversionFormat { get; private set; }

        public VideoRecodeFormat VideoRecodeFormat { get; private set; }

        public CustomDownload(string description) : base(description, false, false)
        { }

        public void Configure(
            FormatData videoFormat, FormatData audioFormat = null,
            bool extractAudio = false,
            AudioConversionFormat audioConversionFormat = AudioConversionFormat.Mp3,
            VideoRecodeFormat videoRecodeFormat = VideoRecodeFormat.None)
        {
            if (extractAudio && audioConversionFormat == AudioConversionFormat.Best)
                throw new InvalidOperationException("Cannot use AudioConversionFormat.Best.");
            if (audioFormat != null && !extractAudio && videoRecodeFormat == VideoRecodeFormat.None)
                throw new InvalidOperationException("Must specify a VideoRecodeFormat when merging formats.");
            this.VideoFormat = videoFormat;
            this.AudioFormat = audioFormat;
            this.IsAudio = extractAudio;
            this.AudioConversionFormat = audioConversionFormat;
            this.VideoRecodeFormat = videoRecodeFormat;
        }

        protected override string GetExt()
        {
            if (IsAudio)
            {
                return ExtProvider.GetExtForAudio(AudioConversionFormat);
            }
            else if (VideoRecodeFormat == VideoRecodeFormat.None)
            {
                if (AudioFormat != null)
                    throw new InvalidOperationException("Must specify a VideoRecodeFormat when merging formats.");
                return VideoFormat.Extension;
            }
            else return ExtProvider.GetExtForVideo(VideoRecodeFormat);
        }

        protected override async Task<RunResult<string>> RunRealDownload(
            YoutubeDL ydl, string url, CancellationToken ct,
            IProgress<DownloadProgress> progress, OptionSet overrideOptions = null)
        {
            if (IsAudio)
            {
                return await ydl.RunAudioDownload(
                    url, AudioConversionFormat, ct, progress,
                    output: new Progress<string>(s => DownloadOutputLogger.Instance.WriteOutput(url, s)),
                    overrideOptions: overrideOptions
                );
            }
            else
            {
                return await ydl.RunVideoDownload(
                    url, FormatSelection,
                    DownloadMergeFormat.Mkv, VideoRecodeFormat, ct, progress,
                    output: new Progress<string>(s => DownloadOutputLogger.Instance.WriteOutput(url, s)),
                    overrideOptions: overrideOptions
                );
            }
        }

        protected override async Task<RunResult<string[]>> RunRealPlaylistDownload(
            YoutubeDL ydl, string url, int[] playlistItems, CancellationToken ct,
            IProgress<DownloadProgress> progress, OptionSet overrideOptions = null)
        {
            if (IsAudio)
            {
                return await ydl.RunAudioPlaylistDownload(url, items: playlistItems,
                    format: AudioConversionFormat, ct: ct, progress: progress,
                    output: new Progress<string>(s => DownloadOutputLogger.Instance.WriteOutput(url, s)),
                    overrideOptions: overrideOptions
                );
            }
            else
            {
                return await ydl.RunVideoPlaylistDownload(url, format: FormatSelection,
                    items: playlistItems, recodeFormat: VideoRecodeFormat, ct: ct, progress: progress,
                    output: new Progress<string>(s => DownloadOutputLogger.Instance.WriteOutput(url, s)),
                    overrideOptions: overrideOptions
                );
            }
        }
    }
}
