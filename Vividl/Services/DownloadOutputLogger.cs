using System;
using System.Text.RegularExpressions;

namespace Vividl.Services
{
    public sealed class DownloadOutputLogger
    {
        private static Regex rgxUrl = new Regex("https?:\\/\\/(www\\.)?", RegexOptions.Compiled);
        private static Regex rgxPwd = new Regex("(--(video-)?password) \".+\"", RegexOptions.Compiled);

        public static DownloadOutputLogger Instance { get; }

        static DownloadOutputLogger()
        {
            Instance = new DownloadOutputLogger();
        }

        private DownloadOutputLogger() { }

        public event EventHandler<DownloadOutputEventArgs> OutputReceived;

        public void WriteOutput(string jobId, string output)
        {
            jobId = rgxUrl.Replace(jobId, "");
            output = rgxPwd.Replace(output, (m) => m.Groups[1] + " ****");
            OutputReceived?.Invoke(this, new DownloadOutputEventArgs(jobId, output));
#if DEBUG
            System.Diagnostics.Debug.WriteLine(output);
#endif
        }
    }

    public class DownloadOutputEventArgs : EventArgs
    {
        public string JobId { get; }

        public string Output { get; }

        public DownloadOutputEventArgs(string jobId, string output)
        {
            this.JobId = jobId;
            this.Output = output;
        }
    }
}
