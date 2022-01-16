using System;
using System.Linq;
using System.Text.RegularExpressions;
using Vividl.Properties;
using WK.Libraries.SharpClipboardNS;

namespace Vividl.Services
{
    /// <summary>
    /// A service class that provides the clipboard automation features of Vividl.
    /// </summary>
    public class SmartAutomationService
    {
        private SharpClipboard clipboard;
        private bool isEnabled;

        public event EventHandler<UrlsEventArgs> InputReceived;

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                if (isEnabled) Start();
                else Stop();
            }
        }

        public void Start()
        {
            if (this.clipboard == null)
            {
                this.clipboard = new SharpClipboard();
                this.clipboard.ClipboardChanged += clipboard_ClipboardChanged;
            }
        }

        public void Stop()
        {
            if (this.clipboard != null)
            {
                this.clipboard.StopMonitoring();
                this.clipboard = null;
            }
        }

        private void clipboard_ClipboardChanged(object sender, SharpClipboard.ClipboardChangedEventArgs e)
        {
            if (e.ContentType == SharpClipboard.ContentTypes.Text)
            {
                string[] urls = clipboard.ClipboardText
                    .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                    .Where(s => Uri.TryCreate(s, UriKind.Absolute, out Uri _))
                    .Where(getMatchFunc()).ToArray();
                if (urls.Length > 0)
                {
                    InputReceived?.Invoke(this, new UrlsEventArgs(urls));
                }
            }
        }

        private Func<string, bool> getMatchFunc()
        {
            if (Settings.Default.SmartAutomationPatternIsRegex)
            {
                Regex regex = new Regex(Settings.Default.SmartAutomationPattern);
                return s => regex.IsMatch(s);
            }
            else
            {
                return s => s.Contains(Settings.Default.SmartAutomationPattern);
            }
        }
    }

    public class UrlsEventArgs : EventArgs
    {
        public UrlsEventArgs(string[] urls)
        {
            this.Urls = urls;
        }

        public string[] Urls { get; }
    }
}
