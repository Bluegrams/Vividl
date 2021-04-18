using System;
using System.Collections.Generic;
using Vividl.Model;
using Vividl.Properties;
using YoutubeDLSharp.Options;

namespace Vividl.Services
{
    public class VideoDownloadOptionProvider : IDownloadOptionProvider
    {
        private const string BEST_MERGE = "bestvideo+bestaudio";
        private const string BEST = "best";

        // based on settings, prefer downloading requested format (faster) or download best and convert to requested (better quality)
        private string getFormatString(string extension)
        {
            if (Settings.Default.PreferRecoding)
                return $"{BEST_MERGE}/{BEST}/{extension}";
            else return $"{extension}/{BEST_MERGE}/{BEST}";
        }

        public List<IDownloadOption> CreateDownloadOptions()
        {
            var options = new List<IDownloadOption>()
            {
                new VideoDownload(getFormatString("mp4"), VideoRecodeFormat.Mp4,
                        description: Resources.DownloadOption_MP4),
                new VideoDownload(getFormatString("webm"), VideoRecodeFormat.Webm,
                        description: Resources.DownloadOption_Webm),
                new VideoDownload("bestvideo+bestaudio/best", recodeFormat: VideoRecodeFormat.Avi,
                        description: Resources.DownloadOption_AVI),
                new VideoDownload("bestvideo+bestaudio/best", recodeFormat: VideoRecodeFormat.Mkv,
                        description: Resources.DownloadOption_MKV),
                new AudioConversionDownload(AudioConversionFormat.Mp3, Resources.DownloadOption_MP3),
                new AudioConversionDownload(AudioConversionFormat.M4a, Resources.DownloadOption_M4A),
                new AudioConversionDownload(AudioConversionFormat.Wav, Resources.DownloadOption_WAV),
                new AudioConversionDownload(AudioConversionFormat.Vorbis, Resources.DownloadOption_Vorbis),
                new VideoDownload("best", description: Resources.DownloadOption_Best),
                new CustomDownload(Resources.DownloadOption_Custom),
            };
            return options;
        }
    }
}
