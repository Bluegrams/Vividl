using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Vividl.Properties;
using Vividl.Services;
using YoutubeDLSharp;
using YoutubeDLSharp.Metadata;
using YoutubeDLSharp.Options;

namespace Vividl.Model
{
    public abstract class DownloadOption : IDownloadOption
    {
        public abstract string FormatSelection { get; }

        public string Description { get; }

        public bool IsAudio { get; protected set; }

        public DownloadOption(string description, bool isAudio)
        {
            this.Description = description;
            this.IsAudio = isAudio;
        }

        /// <summary>
        /// Gets the final file extension of this download format.
        /// This reduces the flexibility of the download process as we cannot support
        /// generic 'best' downloads (see implementations). But this is needed to determine the
        /// final file path for pre-download checks.
        /// </summary>
        public abstract string GetExt(string defaultValue = null);

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
            string ext;
            try
            {
                ext = GetExt(video.Metadata.SelectSingleFormat(FormatSelection)?.Extension);
            }
            catch (InvalidOperationException)
            {
                ext = null;
            }
            // If file ext could not be resolved, we cannot make pre-download checks -> default to overwriting
            if ((Settings.Default.OverwriteMode != OverwriteMode.Overwrite) && (ext != null))
            {
                bool restricted = ydl.RestrictFilenames;
                string fileName = Utils.Sanitize(video.Title, restricted);
                string path = Path.Combine(ydl.OutputFolder, $"{fileName}.{ext}");
                if (File.Exists(path))
                {
                    if (Settings.Default.OverwriteMode == OverwriteMode.None)
                    {
                        // Don't redownload if file exists.
                        progress?.Report(new DownloadProgress(DownloadState.Success, data: path));
                        return new RunResult<string>(true, new string[0], path);
                    }
                    else
                    {
                        // Set download path to a new location to prevent overwriting existing file.
                        string downloadPath = getNotExistingFilePath(ydl.OutputFolder, fileName, ext);
                        if (overrideOptions == null)
                        {
                            overrideOptions = new OptionSet();
                        }
                        overrideOptions.Output = downloadPath;
                    }
                }
            }
            return await RunRealDownload(ydl, video.Url, ct, progress, overrideOptions);
        }

        /// <summary>
        /// Adds an incrementing counter after the file name until it found a non-existing file.
        /// </summary>
        private string getNotExistingFilePath(string folder, string fileName, string ext)
        {
            int i = 1;
            while (i < 256)
            {
                string filePath = Path.Combine(folder, $"{fileName} ({i}).{ext}");
                if (!File.Exists(filePath))
                {
                    return filePath;
                }
                i += 1;
            }
            // Stop trying to prevent long running
            throw new InvalidOperationException($"Cannot find available file path for {fileName}.");
        }

        /// <summary>
        /// Performs the download of the specified playlist with the given YoutubeDL instance and a prior overwrite check.
        /// </summary>
        public async Task<RunResult<string[]>> RunDownload(YoutubeDL ydl, PlaylistEntry playlist,
            CancellationToken ct, IProgress<DownloadProgress> progress, OptionSet overrideOptions = null)
        {
            string ext;
            try
            {
                ext = GetExt();
            }
            catch (InvalidOperationException)
            {
                ext = null;
            }
            // If file ext could not be resolved, we cannot make pre-download checks -> default to overwriting
            // TODO OverwriteMode.Increment currently not working for playlists.
            if ((Settings.Default.OverwriteMode != OverwriteMode.Overwrite) && (ext != null))
            {
                bool restricted = ydl.RestrictFilenames;
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
            : base(description, true)
        {
            this.ConversionFormat = format;
        }

        public override string GetExt(string defaultValue = null)
            => ExtProvider.GetExtForAudio(ConversionFormat, defaultValue);

        protected override async Task<RunResult<string>> RunRealDownload(YoutubeDL ydl, string url,
            CancellationToken ct, IProgress<DownloadProgress> progress, OptionSet overrideOptions = null)
        {
            overrideOptions = DownloadConfigurations.ApplyForAudioDownload(this, overrideOptions);
            return await ydl.RunAudioDownload(
                url, ConversionFormat, ct, progress,
                output: new Progress<string>(s => DownloadOutputLogger.Instance.WriteOutput(url, s)),
                overrideOptions: overrideOptions
            );
        }

        protected override async Task<RunResult<string[]>> RunRealPlaylistDownload(YoutubeDL ydl, string url,
            int[] playlistItems, CancellationToken ct, IProgress<DownloadProgress> progress, OptionSet overrideOptions = null)
        {
            overrideOptions = DownloadConfigurations.ApplyForAudioDownload(this, overrideOptions);
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
            : base(description, isAudio)
        {
            this.FormatSelection = formatSelection;
            this.RecodeFormat = recodeFormat;
            this.fileExtension = fileExtension;
        }

        public override string GetExt(string defaultValue = null)
        {
            if (!String.IsNullOrWhiteSpace(fileExtension))
                return fileExtension;
            return ExtProvider.GetExtForVideo(RecodeFormat, defaultValue);
        }

        // yt-dlp gives a warning if we pass "best", therefore change to "b" before download.
        private string fixFormatSelection(YoutubeDL ydl, string formatSelection)
            => (((CustomYoutubeDL)ydl).UsingYtDlp && FormatSelection == "best") ? "b" : formatSelection;

        protected override async Task<RunResult<string>> RunRealDownload(YoutubeDL ydl, string url,
            CancellationToken ct, IProgress<DownloadProgress> progress, OptionSet overrideOptions = null)
        {
            string formatSelection = fixFormatSelection(ydl, FormatSelection);
            overrideOptions = DownloadConfigurations.ApplyForVideoDownload(this, overrideOptions);
            return await ydl.RunVideoDownload(
                url, formatSelection,
                DownloadMergeFormat.Mkv, RecodeFormat, ct, progress,
                output: new Progress<string>(s => DownloadOutputLogger.Instance.WriteOutput(url, s)),
                overrideOptions: overrideOptions
            );
        }

        protected override async Task<RunResult<string[]>> RunRealPlaylistDownload(YoutubeDL ydl, string url,
            int[] playlistItems, CancellationToken ct, IProgress<DownloadProgress> progress, OptionSet overrideOptions = null)
        {
            string formatSelection = fixFormatSelection(ydl, FormatSelection);
            overrideOptions = DownloadConfigurations.ApplyForVideoDownload(this, overrideOptions);
            return await ydl.RunVideoPlaylistDownload(url, format: formatSelection,
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
                else if (VideoFormat == null) return AudioFormat?.FormatId;
                else return VideoFormat.FormatId + "+" + AudioFormat.FormatId;
            }
        }

        public AudioConversionFormat AudioConversionFormat { get; private set; }

        public VideoRecodeFormat VideoRecodeFormat { get; private set; }

        public CustomDownload(string description) : base(description, false)
        { }

        public void Configure(
            FormatData videoFormat, FormatData audioFormat = null,
            bool extractAudio = false,
            AudioConversionFormat audioConversionFormat = AudioConversionFormat.Mp3,
            VideoRecodeFormat videoRecodeFormat = VideoRecodeFormat.None)
        {
            if (videoFormat == null && audioFormat == null)
                throw new InvalidOperationException(Resources.DownloadOption_Exception_NoFormat);
            if (extractAudio && audioConversionFormat == AudioConversionFormat.Best)
                throw new InvalidOperationException("Cannot use AudioConversionFormat.Best.");
            if (audioFormat != null && !extractAudio && videoRecodeFormat == VideoRecodeFormat.None)
                throw new InvalidOperationException(Resources.DownloadOption_Exception_RecodeFormat);
            if (audioFormat == null && videoFormat.AudioCodec == "none" && extractAudio)
                throw new InvalidOperationException(Resources.DownloadOption_Exception_NoAudio);
            this.VideoFormat = videoFormat;
            this.AudioFormat = audioFormat;
            this.IsAudio = extractAudio;
            this.AudioConversionFormat = audioConversionFormat;
            this.VideoRecodeFormat = videoRecodeFormat;
        }

        public override string GetExt(string defaultValue = null)
        {
            if (IsAudio)
            {
                return ExtProvider.GetExtForAudio(AudioConversionFormat, defaultValue);
            }
            else if (VideoRecodeFormat == VideoRecodeFormat.None)
            {
                if (AudioFormat != null)
                    throw new InvalidOperationException("Must specify a VideoRecodeFormat when merging formats.");
                return VideoFormat?.Extension;
            }
            else return ExtProvider.GetExtForVideo(VideoRecodeFormat, defaultValue);
        }

        protected override async Task<RunResult<string>> RunRealDownload(
            YoutubeDL ydl, string url, CancellationToken ct,
            IProgress<DownloadProgress> progress, OptionSet overrideOptions = null)
        {
            if (IsAudio)
            {
                overrideOptions = DownloadConfigurations.ApplyForAudioDownload(this, overrideOptions);
                return await ydl.RunAudioDownload(
                    url, AudioConversionFormat, ct, progress,
                    output: new Progress<string>(s => DownloadOutputLogger.Instance.WriteOutput(url, s)),
                    overrideOptions: overrideOptions
                );
            }
            else
            {
                overrideOptions = DownloadConfigurations.ApplyForVideoDownload(this, overrideOptions);
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
                overrideOptions = DownloadConfigurations.ApplyForAudioDownload(this, overrideOptions);
                return await ydl.RunAudioPlaylistDownload(url, items: playlistItems,
                    format: AudioConversionFormat, ct: ct, progress: progress,
                    output: new Progress<string>(s => DownloadOutputLogger.Instance.WriteOutput(url, s)),
                    overrideOptions: overrideOptions
                );
            }
            else
            {
                overrideOptions = DownloadConfigurations.ApplyForVideoDownload(this, overrideOptions);
                return await ydl.RunVideoPlaylistDownload(url, format: FormatSelection,
                    items: playlistItems, recodeFormat: VideoRecodeFormat, ct: ct, progress: progress,
                    output: new Progress<string>(s => DownloadOutputLogger.Instance.WriteOutput(url, s)),
                    overrideOptions: overrideOptions
                );
            }
        }
    }
}
