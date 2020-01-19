using System;
using System.ComponentModel;
using System.Threading.Tasks;
using YoutubeDLSharp;

namespace Vividl.Services
{
    public interface ILibUpdateService
    {
        string Version { get; }
        bool IsUpdating { get; }
        Task<string> Update();
    }

    public class YtdlUpdateService : ILibUpdateService, INotifyPropertyChanged
    {
        private YoutubeDL ydl;
        private bool isUpdating;

        public YtdlUpdateService(YoutubeDL ydl)
        {
            this.ydl = ydl;
        }

        public string Version => ydl.Version;

        public bool IsUpdating
        {
            get => isUpdating;
            set
            {
                isUpdating = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUpdating)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task<string> Update()
        {
            IsUpdating = true;
            var output = await ydl.RunUpdate();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Version)));
            IsUpdating = false;
            return output;
        }
    }
}
