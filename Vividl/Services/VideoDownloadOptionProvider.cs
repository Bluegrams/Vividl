﻿using System;
using System.Collections.Generic;
using Vividl.Model;
using Vividl.Properties;
using YoutubeDLSharp.Options;

namespace Vividl.Services
{
    public class VideoDownloadOptionProvider : IDownloadOptionProvider
    {
        // based on settings, prefer downloading requested format (faster) or download best and convert to requested (better quality)
        private string getFormatString(string extension)
        {
            string s;
            if (App.UsingYtDlp)
            {
                s = "bv*+ba/b";
            }
            else
            {
                if (Settings.Default.PreferRecoding)
                    s = "bestvideo+bestaudio/best";
                else s = $"best/bestvideo+bestaudio";
            }
            if (!Settings.Default.PreferRecoding && extension != null)
            {
                s = extension + "[acodec!=none]/" + s;
            }
            return s;
        }

        public List<IDownloadOption> CreateDownloadOptions(bool withCustomDownload = false)
        {
            var options = new List<IDownloadOption>()
            {
                new VideoDownload(getFormatString("mp4"), VideoRecodeFormat.Mp4,
                        description: Resources.DownloadOption_MP4),
                new VideoDownload(getFormatString("webm"), VideoRecodeFormat.Webm,
                        description: Resources.DownloadOption_Webm),
                new VideoDownload(getFormatString(null), recodeFormat: VideoRecodeFormat.Avi,
                        description: Resources.DownloadOption_AVI),
                new VideoDownload(getFormatString(null), recodeFormat: VideoRecodeFormat.Mkv,
                        description: Resources.DownloadOption_MKV),
                new AudioConversionDownload(AudioConversionFormat.Mp3, Resources.DownloadOption_MP3),
                new AudioConversionDownload(AudioConversionFormat.M4a, Resources.DownloadOption_M4A),
                new AudioConversionDownload(AudioConversionFormat.Wav, Resources.DownloadOption_WAV),
                new AudioConversionDownload(AudioConversionFormat.Vorbis, Resources.DownloadOption_Vorbis),
                new VideoDownload("best", description: Resources.DownloadOption_Best),
                new VideoDownload("bestaudio", description: Resources.DownloadOption_BestAudio, isAudio: true),
            };
            if (withCustomDownload)
            {
                options.Add(new CustomDownload(Resources.DownloadOption_Custom));
            }
            return options;
        }
    }
}
