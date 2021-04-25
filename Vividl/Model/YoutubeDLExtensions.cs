using System;
using System.Collections.Generic;
using System.Linq;
using YoutubeDLSharp.Metadata;

namespace Vividl.Model
{
    public static class YoutubeDLExtensions
    {
        public static string GetWidthAndHeight(this FormatData formatData)
            => formatData.Width != null && formatData.Height != null ? $"{formatData.Width} x {formatData.Height}" : null;

        public static IEnumerable<FormatData> GetAudioVideoFormats(this VideoData videoData)
            => videoData.Formats.Where(f => f.VideoCodec != "none" && f.AudioCodec != "none");

        public static IEnumerable<FormatData> GetAudioOnlyFormats(this VideoData videoData)
            => videoData.Formats.Where(f => f.VideoCodec == "none");

        public static IEnumerable<FormatData> GetVideoOnlyFormats(this VideoData videoData)
            => videoData.Formats.Where(f => f.AudioCodec == "none");

        /// <summary>
        /// Selects a format from the list of available formats based on the given format specifier (single).
        /// Ported from _build_selector_function() in youtube_dl/YoutubeDL.py:
        /// https://github.com/ytdl-org/youtube-dl/blob/9c1e164e0cd77331ea4f0b474b32fd06f84bad71/youtube_dl/YoutubeDL.py#L1262-L1316.
        /// </summary>
        /// <returns>The selected format or null if not found.</returns>
        public static FormatData SelectSingleFormat(this VideoData videoData, string formatSpecifier)
        {
            if (new[] { "best", "worst" , null }.Contains(formatSpecifier))
            {
                var audioVideoFormats = videoData.GetAudioVideoFormats().ToList();
                if (audioVideoFormats.Count > 0)
                {
                    int index = formatSpecifier == "worst" ? 0 : (audioVideoFormats.Count - 1);
                    return audioVideoFormats[index];
                }
                // select best video-only or audio-only format
                else
                {
                    int index = formatSpecifier == "worst" ? 0 : (videoData.Formats.Length - 1);
                    return videoData.Formats[index];
                }
            }
            else if (formatSpecifier == "bestaudio")
            {
                var audioFormats = videoData.GetAudioOnlyFormats().ToList();
                if (audioFormats.Count > 0)
                    return audioFormats[audioFormats.Count - 1];
                else return null;
            }
            else if (formatSpecifier == "worstaudio")
            {
                var audioFormats = videoData.GetAudioOnlyFormats().ToList();
                if (audioFormats.Count > 0)
                    return audioFormats[0];
                else return null;
            }
            else if (formatSpecifier == "bestvideo")
            {
                var audioFormats = videoData.GetVideoOnlyFormats().ToList();
                if (audioFormats.Count > 0)
                    return audioFormats[audioFormats.Count - 1];
                else return null;
            }
            else if (formatSpecifier == "worstvideo")
            {
                var audioFormats = videoData.GetVideoOnlyFormats().ToList();
                if (audioFormats.Count > 0)
                    return audioFormats[0];
                else return null;
            }
            else
            {
                string[] extensions = new[] { "mp4", "flv", "webm", "3gp", "m4a", "mp3", "ogg", "aac", "wav" };
                if (extensions.Contains(formatSpecifier))
                {
                    var matches = videoData.Formats.Where(f => f.Extension == formatSpecifier);
                    return matches.LastOrDefault();
                }
                else
                {
                    var matches = videoData.Formats.Where(f => f.FormatId == formatSpecifier);
                    return matches.LastOrDefault();
                }
            }
        }

        /// <summary>
        /// Selects formats based on a format specifier from the list of available formats.
        /// Note that this only supports single formats, precedence lists ("/") and merges ("+").
        /// </summary>
        /// <returns>The selected formats or null if none existing.</returns>
        public static FormatData[] SelectFormat(this VideoData videoData, string formatSpecifier)
        {
            // 1. Split by precedence list: "/"
            foreach (string group in formatSpecifier.Split('/'))
            {
                // 2. Split merging group: "+"
                string[] splits = group.Split('+');
                if (splits.Length == 1)
                {
                    FormatData format = videoData.SelectSingleFormat(splits[0]);
                    if (format != null)
                        return new[] { format };
                    else continue;
                }
                else if (splits.Length == 2)
                {
                    FormatData video = videoData.SelectSingleFormat(splits[0]);
                    FormatData audio = videoData.SelectSingleFormat(splits[1]);
                    // continue with next option if one of video or audio is not existing
                    if (video == null || audio == null)
                        continue;
                    if (video.VideoCodec == "none")
                        throw new ArgumentException("The first format in a merge must contain a video.");
                    if (audio.AudioCodec == "none")
                        throw new ArgumentException("The second format in a merge must contain an audio.");
                    return new[] { video, audio };
                }
                else throw new ArgumentException($"Invalid formar specifier {formatSpecifier}");
            }
            // we have found none of the given formats
            return null;
        }
    }
}
