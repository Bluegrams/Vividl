using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using YoutubeDLSharp.Metadata;

namespace Vividl.Model
{
    public static class YoutubeDLExtensions
    {
        // Adapted from https://github.com/yt-dlp/yt-dlp/blob/85b33f5c163f60dbd089a6b9bc2ba1366d3ddf93/yt_dlp/utils/_utils.py#L4996
        private static readonly string[] COMMON_VIDEO = new[] { "3gp", "avi", "flv", "mkv", "mov", "mp4", "webm" };
        private static readonly string[] COMMON_AUDIO = new[] { "aiff", "alac", "flac", "m4a", "mka", "mp3", "ogg", "opus", "wav" };

        // Adapted from https://github.com/yt-dlp/yt-dlp/blob/85b33f5c163f60dbd089a6b9bc2ba1366d3ddf93/yt_dlp/YoutubeDL.py#L2415-L2417
        private static readonly Regex rgxSelection = new Regex(
            @"(?<bw>best|worst|b|w)(?<type>video|audio|v|a)?(?<mod>\*)?(?:\.(?<n>[1-9]\d*))?$",
            RegexOptions.Compiled
        );

        // Copied from yt-dlp documentation (removing res)
        private const string DEFAULT_SORT_ORDER = "lang,quality,fps,hdr:12,vcodec:vp9.2,channels,acodec,size,br,asr,proto,ext,hasaud,source,id";

        public static string GetWidthAndHeight(this FormatData formatData)
            => formatData.Width != null && formatData.Height != null ? $"{formatData.Width} x {formatData.Height}" : null;

        public static IEnumerable<FormatData> GetAudioVideoFormats(this FormatData[] formats)
            => formats.Where(f => f.VideoCodec != "none" && f.AudioCodec != "none");

        public static IEnumerable<FormatData> GetAudioOnlyFormats(this FormatData[] formats)
            => formats.Where(f => f.VideoCodec == "none" && f.AudioCodec != "none");

        public static IEnumerable<FormatData> GetVideoOnlyFormats(this FormatData[] formats)
            => formats.Where(f => f.VideoCodec != "none" && f.AudioCodec == "none");

        /// <summary>
        /// Selects a format from the list of available formats based on the given format specifier (single).
        /// Ported from _build_selector_function() in youtube_dl/YoutubeDL.py:
        /// https://github.com/yt-dlp/yt-dlp/blob/85b33f5c163f60dbd089a6b9bc2ba1366d3ddf93/yt_dlp/YoutubeDL.py#L2395-L2463.
        /// </summary>
        /// <returns>The selected format or null if not found.</returns>
        public static FormatData SelectSingleFormat(this VideoData videoData, string formatSpecifier)
        {
            Func<FormatData, bool> filterF;
            Match match = rgxSelection.Match(formatSpecifier);
            bool formatReverse = true;
            int formatIdx = 1;
            if (match.Success)
            {
                if (!int.TryParse(match.Groups["n"].Value, out formatIdx))
                    formatIdx = 1;
                formatReverse = match.Groups["bw"].Value[0] == 'b';
                string formatType = match.Groups["type"].Success ? match.Groups["type"].Value[0].ToString() : null;
                string notFormatType = formatType == "v" ? "a" : "v";
                bool formatModified = match.Groups["mod"].Success;
                bool formatFallback = formatType == null && !formatModified;
                Func<FormatData, bool> _filterF;

                // bv*, ba*, wv*, wa*
                if (formatType != null && formatModified)
                {
                    if (formatType == "v") _filterF = (f) => f.VideoCodec != "none";
                    else _filterF = (f) => f.AudioCodec != "none";
                }
                // bv, ba, wv, wa
                else if (formatType != null)
                {
                    if (formatType == "v") _filterF = (f) => f.AudioCodec == "none";
                    else _filterF = (f) => f.VideoCodec == "none";
                }
                // b, w
                else if (!formatModified)
                    _filterF = (f) => f.VideoCodec != "none" && f.AudioCodec != "none";
                // b*, w*
                else _filterF = (f) => true;
                filterF = (f) => _filterF(f) && (f.VideoCodec != "none" || f.AudioCodec != "none");
            }
            else
            {
                if (COMMON_VIDEO.Contains(formatSpecifier) || COMMON_AUDIO.Contains(formatSpecifier))
                    filterF = (f) => f.Extension == formatSpecifier;
                else filterF = (f) => f.FormatId == formatSpecifier;
            }

            // eqv. to selector_function()
            var matches = videoData.Formats.Where(filterF);
            if (formatReverse)
                matches = matches.Reverse();
            if (matches.Count() >= formatIdx)
                return matches.ToList()[formatIdx - 1];
            else return null;
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

        /// <summary>
        /// Converts a Resolution enum object to a yt-dlp format sorting string.
        /// </summary>
        /// <returns>A format sorting string which can be passed to --format-sort.</returns>
        public static string ToFormatSort(this Resolution resolution)
        {
            string formatSort;
            switch (resolution)
            {
                case Resolution.ResMin:
                    formatSort = "+res";
                    break;
                case Resolution.ResMax:
                    formatSort = "res";
                    break;
                default:
                    formatSort = $"res:{(int)resolution}";
                    break;
            }
            return formatSort + "," + DEFAULT_SORT_ORDER;
        }
    }
}
