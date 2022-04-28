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
            if (download.GetExt() == "mp4")
            {
                switch (Settings.Default.FFmpegHardwareAcceleration)
                {
                    case HwAccelMode.NvidiaCuda:
                        options = options ?? new OptionSet();
                        options.PostprocessorArgs = "ffmpeg:-vcodec h264_nvenc"; // Use CUDA-based H.264 encoder for MP4
                        options.AddCustomOption("--postprocessor-args", "ffmpeg_i1:-hwaccel cuda -hwaccel_output_format cuda"); // Add another post-processor option for input file args
                        break;
                    case HwAccelMode.AmdAmf:
                        // AMD support is still in beta
                        options = options ?? new OptionSet();
                        options.PostprocessorArgs = "ffmpeg:-vcodec h264_amf"; // Use AMD-based H.264 encoder for MP4
                        options.AddCustomOption("--postprocessor-args", "ffmpeg_i1:-hwaccel auto"); // Add another post-processor option for input file args
                        break;
                }
            }
            return options;
        }
    }
}