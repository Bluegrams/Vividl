using System;
using System.Text.RegularExpressions;

namespace Vividl.Model
{
    class VideoEntryException : Exception
    {
        private static Regex rgxSent = new Regex("ERROR: (.*?)[\\.;:](?:\\s|$)", RegexOptions.Compiled);

        public string FirstSentence { get; }

        public VideoEntryException(string[] errorLines)
            : base(String.Join(Environment.NewLine, errorLines))
        {
            var match = rgxSent.Match(String.Join(Environment.NewLine, errorLines));
            FirstSentence = match.Groups[1].Value;
        }
    }
}
