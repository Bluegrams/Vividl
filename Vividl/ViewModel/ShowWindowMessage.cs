
using System;

namespace Vividl.ViewModel
{
    public enum WindowType
    {
        VideoDataWindow,
        PlaylistDataWindow,
        SettingsWindow,
        FetchWindow,
        DownloadOutputWindow,
        FormatSelectionWindow
    }


    internal class ShowWindowMessage
    {
        public WindowType Window { get; }
        public object Parameter { get; }
        public Action<bool?, object> Callback { get; }

        public ShowWindowMessage(WindowType window, object parameter = null,
            Action<bool?, object> callback = null)
        {
            this.Window = window;
            this.Parameter = parameter;
            this.Callback = callback;
        }
    }
}
