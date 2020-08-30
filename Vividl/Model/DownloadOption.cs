using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vividl.Services;
using YoutubeDLSharp;
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

        public string Description { get; }

        public bool IsAudio { get; }

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
        public AudioConversionFormat ConversionFormat { get; }

        public AudioConversionDownload(AudioConversionFormat format,
            string description = null)
            : base(description, true, true)
        {
            this.ConversionFormat = format;
        }

        protected override string GetExt()
        {
            switch (ConversionFormat)
            {
                case AudioConversionFormat.Mp3:
                    return "mp3";
                case AudioConversionFormat.M4a:
                    return "m4a";
                case AudioConversionFormat.Vorbis:
                    return "ogg";
                case AudioConversionFormat.Wav:
                    return "wav";
                case AudioConversionFormat.Opus:
                    return "opus";
                case AudioConversionFormat.Aac:
                    return "aac";
                case AudioConversionFormat.Flac:
                    return "flac";
                default:
                    // Don't support 'best' because we don't know the extension in advance!
                    throw new InvalidOperationException("AudioConversionFormat.Best is not supported.");
            }
        }

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
        public string FormatSelection { get; }
        public VideoRecodeFormat RecodeFormat { get; }

        /* Use this to manually specify the file extension of this download option.
         * If this field is empty, we try to infer the file extension from the selected video format. */
        private string fileExtension;

        public VideoDownload(string formatSelection,
                            VideoRecodeFormat recodeFormat = VideoRecodeFormat.None,
                            string description = null, string fileExtension = "")
            : base(description, false, !String.IsNullOrEmpty(fileExtension) || recodeFormat != VideoRecodeFormat.None)
        {
            this.FormatSelection = formatSelection;
            this.RecodeFormat = recodeFormat;
            this.fileExtension = fileExtension;
        }

        protected override string GetExt()
        {
            if (!String.IsNullOrWhiteSpace(fileExtension))
                return fileExtension;
            switch (RecodeFormat)
            {
                case VideoRecodeFormat.Avi:
                    return "avi";
                case VideoRecodeFormat.Mp4:
                    return "mp4";
                case VideoRecodeFormat.Ogg:
                    return "ogg";
                case VideoRecodeFormat.Flv:
                    return "flv";
                case VideoRecodeFormat.Webm:
                    return "webm";
                case VideoRecodeFormat.Mkv:
                    return "mkv";
                default:
                    // Don't support 'None' because we don't know the extension in advance!
                    throw new InvalidOperationException("VideoRecodeFormat.None is not supported.");
            }
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
}
