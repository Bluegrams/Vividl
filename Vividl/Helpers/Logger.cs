using System;
using System.IO;
using Bluegrams.Application;

namespace Vividl.Helpers
{
    internal class Logger
    {
        public static Logger Default { get; } = new Logger();

        string file;

        private Logger()
        {
            file = Path.Combine(Path.GetTempPath(), AppInfo.ProductName.ToLower() + ".log");
        }

        public void Log(string message, Exception ex)
        {
            string logEntry = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}] {message}";
            File.AppendAllLines(file, new[] { logEntry, ex.ToString()});
        }
    }
}
