using System;
using System.Collections.Generic;
using System.Linq;
using Bluegrams.Application;
using GalaSoft.MvvmLight;
using System.Threading.Tasks;
using System.Windows.Input;
using Vividl.Model;
using Vividl.Services;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Vividl.Properties;
using GalaSoft.MvvmLight.Ioc;
using YoutubeDLSharp.Options;

namespace Vividl.ViewModel
{
    public abstract class ItemViewModel<T> : ViewModelBase where T : IDownloadEntry
    {
        readonly string url;
        T entry;
        float currentProgress;
        string progressString, totalDownloadSize, downloadSpeed, downloadTimeRemaining;
        // downloadIndex indicates the number of the _next_ item to be downloaded
        int downloadIndex = 1;
        ItemState state;
        bool unavailable = false;

        protected MainViewModel<T> mainVm;
        protected IDialogService messageService;
        protected IFileService fileService;
        // this should be set to the number of default download options during initialization
        protected int defaultOptionsCount;

        public T Entry
        {
            get => entry;
            protected set
            {
                entry = value;
                RaisePropertyChanged();
            }
        }

        public ICommand DownloadCommand { get; }

        public ICommand ShowMetadataCommand { get; }

        public ICommand CancelCommand { get; }

        public ICommand CopyClipboardCommand { get; }

        public ICommand OpenInBrowserCommand { get; }

        public ICommand ShowInFolderCommand { get; }

        public ICommand PlayCommand { get; }

        public ICommand ReloadCommand { get; }

        public ICommand DeleteCommand { get; }

        // TODO Rename: IsPlaylist -> IsCollection
        public abstract bool IsPlaylist { get; }

        public IList<IDownloadOption> DownloadOptions
        {
            get
            {
                if (Settings.Default.ShowAllFormats) return Entry.DownloadOptions;
                else return Entry.DownloadOptions.Take(defaultOptionsCount).ToList();
            }
        }

        public int SelectedDownloadOption
        {
            get => Entry.SelectedDownloadOption;
            set
            {
                Entry.SelectedDownloadOption = value;
                RaisePropertyChanged();
            }
        }

        public bool IsDownloading { get; private set; }

        public float CurrentProgress
        {
            get => currentProgress;
            protected set
            {
                currentProgress = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// String describing download progress above the progress bar.
        /// </summary>
        public string ProgressString
        {
            get => progressString;
            protected set
            {
                progressString = value;
                RaisePropertyChanged();
            }
        }

        public string TotalDownloadSize
        {
            get => totalDownloadSize;
            protected set
            {
                totalDownloadSize = value;
                RaisePropertyChanged();
            }
        }

        public string DownloadSpeed
        {
            get => downloadSpeed;
            protected set
            {
                downloadSpeed = value;
                RaisePropertyChanged();
            }
        }

        public string DownloadTimeRemaining
        {
            get => downloadTimeRemaining;
            protected set
            {
                downloadTimeRemaining = value;
                RaisePropertyChanged();
            }
        }

        public ItemState State
        {
            get => state;
            protected set
            {
                state = value;
                RaisePropertyChanged();
                IsDownloading = value == ItemState.Downloading;
                RaisePropertyChanged(nameof(IsDownloading));
            }
        }

        public bool Unavailable
        {
            get => unavailable;
            protected set
            {
                unavailable = value;
                RaisePropertyChanged();
            }
        }

        public abstract string InformationString { get; }

        public string DownloadIndexString => String.Format("{0} / {1}", downloadIndex, Entry.TotalItems);

        protected void incrDownloadIndex()
        {
            downloadIndex++;
            RaisePropertyChanged(nameof(DownloadIndexString));
        }

        public ItemViewModel(string url, MainViewModel<T> mainVm)
        {
            this.url = url;
            this.mainVm = mainVm;
            this.messageService = SimpleIoc.Default.GetInstance<IDialogService>();
            this.fileService = SimpleIoc.Default.GetInstance<IFileService>();
            CurrentProgress = 0;
            // Init commands
            DownloadCommand = new RelayCommand(async () => await DownloadVideo(),
                () => State == ItemState.Fetched, true);
            ShowMetadataCommand = new RelayCommand(
                () => Messenger.Default.Send(
                    new ShowWindowMessage(IsPlaylist ? WindowType.PlaylistDataWindow : WindowType.VideoDataWindow, this)),
                () => State != ItemState.None
                );
            CancelCommand = new RelayCommand(() => Entry.CancelDownload());
            CopyClipboardCommand = new RelayCommand(() => Clipboard.SetText(this.url));
            OpenInBrowserCommand = new RelayCommand(() => Entry.OpenInBrowser(),
                () => State != ItemState.None);
            ShowInFolderCommand = new RelayCommand(() => Entry.ShowInFolder(fileService),
                () => State == ItemState.Succeeded && Entry.FileAvailable);
            PlayCommand = new RelayCommand(() => Entry.OpenFile(),
                () => State == ItemState.Succeeded && Entry.FileAvailable);
            ReloadCommand = new RelayCommand(async () => await Reload(),
                () => State == ItemState.Succeeded || Unavailable);
            DeleteCommand = new RelayCommand(() => mainVm.Delete(this));
        }

        public abstract Task Fetch(bool refetch = false, OptionSet overrideOptions = null);

        public async Task DownloadVideo()
        {
            if (State != ItemState.Fetched) return;
            CurrentProgress = 0;
            mainVm.SetStats(finished: false);
            State = ItemState.Downloading;
            DownloadResult result = await Entry.Download();
            switch (result)
            {
                case DownloadResult.Success:
                    State = ItemState.Succeeded;
                    mainVm.SetStats(finished: true, success: true);
                    break;
                case DownloadResult.Cancelled:
                    State = ItemState.Fetched;
                    Messenger.Default.Send(
                        new NotificationMessage(String.Format(Resources.Video_DownloadCancelled, Entry.Title)));
                    int remaining = Entry.TotalItems - downloadIndex + 1;
                    mainVm.SetStats(finished: true, success: false);
                    break;
                case DownloadResult.Failed:
                    State = ItemState.Fetched;
                    mainVm.SetStats(finished: true, success: false);
                    Messenger.Default.Send(
                        new NotificationMessage(String.Format(Resources.Video_DownloadFailed, Entry.Title)));
                    break;
            }
            // Manually update CanExecute state of commands.
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Refetches the item.
        /// </summary>
        /// <returns></returns>
        public async Task Reload()
        {
            Entry?.CancelDownload();
            State = ItemState.None;
            Unavailable = false;
            await Fetch(refetch: true);
        }

        /// <summary>
        /// Updates the listed download formats according to the currently selected preferences.
        /// </summary>
        public void UpdateCurrentFormats()
        {
            // only execute if we can access the download options list
            if (State != ItemState.Fetched)
                return;
            RaisePropertyChanged(nameof(DownloadOptions));
            // if we disabled display of all formats, the current index can exceed the total items
            // in this case, select the (new) default format
            if (SelectedDownloadOption > (DownloadOptions.Count - 1))
            {
                SelectedDownloadOption = Settings.Default.DefaultFormat;
            }
        }

        public override string ToString() => url;
    }
}
