using System;
using Vividl.Properties;
using YoutubeDLSharp.Options;

namespace Vividl.Model
{
    public static class DownloadConfigurations
    {
        public static OptionSet ApplyForAudioDownload(DownloadOption download, OptionSet options)
        {
            // When converting to mp3, add thumbnail.
            if (Settings.Default.AddMetadata && download.GetExt() == "mp3")
            {
                options = options ?? new OptionSet();
                options.EmbedThumbnail = true;
                // This ensures thumbnails are correctly shown on Windows.
                options.PostprocessorArgs = "-id3v2_version 3";
            }
            return options;
        }

        public static OptionSet ApplyForVideoDownload(DownloadOption download, OptionSet options)
        {
            if (Settings.Default.FFmpegCudaAcceleration && download.GetExt() == "mp4")
            {
                options = options ?? new OptionSet();
                // Use CUDA-based H.264 encoder for MP4
                options.PostprocessorArgs = "ffmpeg:-vcodec h264_nvenc";
                // Add another post-processor option for input file args
                options.AddCustomOption("--postprocessor-args", "ffmpeg_i1:-hwaccel cuda -hwaccel_output_format cuda");
            }
            return options;
        }
    }
}
