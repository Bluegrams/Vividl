using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Bluegrams.Application;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Vividl.Model;
using Vividl.Properties;
using Vividl.Services;
using Enterwell.Clients.Wpf.Notifications;

namespace Vividl.ViewModel
{
    public class MainViewModel<T> : ViewModelBase where T : IDownloadEntry
    {
        ObservableCollection<ItemViewModel<T>> videoInfos;
        IItemProvider<T> itemProvider;
        IDialogService dialogService;
        IFileService fileService;
        int inProcessCount, successCount, failedCount;
        SmartAutomationService automationService;

        public INotificationMessageManager NotificationManager { get; }

        public ObservableCollection<ItemViewModel<T>> VideoInfos
        {
            get => videoInfos;
            set
            {
                videoInfos = value;
                RaisePropertyChanged();
            }
        }

        public ICommand FetchCommand { get; }

        public ICommand ClearCommand { get; }

        public ICommand PasteCommand { get; }

        public ICommand DropCommand { get; }

        public ICommand ImportCommand { get; }

        public ICommand ExportCommand { get; }

        public ICommand ExitCommand { get; }

        public ICommand ReloadAllCommand { get; }

        public ICommand DownloadAllCommand { get; }

        public ICommand CancelAllCommand { get; }

        public ICommand RemoveUnavailableCommand { get; }

        public ICommand RemoveFinishedCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand SmartAutomationCommand { get; }

        public ICommand SettingsCommand { get; }

        public ICommand OpenDownloadFolderCommand { get; }

        public ICommand SelectDownloadFolderCommand { get; }

        public ICommand ShowDownloadOutputWindowCommand { get; }

        public ICommand OpenErrorLogCommand { get; }

        public ICommand CheckForUpdatesCommand { get; }

        public ICommand AboutCommand { get; }

        // Statistics
        public int InProcessCount
        {
            get => inProcessCount;
            set
            {
                inProcessCount = value;
                RaisePropertyChanged();
            }
        }
        public int SuccessCount
        {
            get => successCount;
            set
            {
                successCount = value;
                RaisePropertyChanged();
            }
        }
        public int FailedCount
        {
            get => failedCount;
            set
            {
                failedCount = value;
                RaisePropertyChanged();
            }
        }

        // Automate
        public bool SmartAutomationMode
        {
            get => automationService.IsEnabled;
            set
            {
                automationService.IsEnabled = value;
                RaisePropertyChanged();
            }
        }

        public MainViewModel(IItemProvider<T> itemProvider, IDialogService dialogService,
            IFileService fileService, INotificationMessageManager notificationManager,
            SmartAutomationService automationService)
        {
            this.itemProvider = itemProvider;
            this.dialogService = dialogService;
            this.fileService = fileService;
            this.NotificationManager = notificationManager;
            this.automationService = automationService;
            this.automationService.InputReceived += automationService_InputReceived;
            VideoInfos = new ObservableCollection<ItemViewModel<T>>();
            // Init commands
            FetchCommand = new RelayCommand(
                () => Messenger.Default.Send(
                    new ShowWindowMessage(WindowType.FetchWindow,
                    parameter: new FetchViewModel(), callback: fetchCommandCallback)
                )
            );
            ClearCommand = new RelayCommand(() => this.Clear(), () => InProcessCount < 1);
            PasteCommand = new RelayCommand(async () => await FetchVideo(Clipboard.GetText()));
            DropCommand = new RelayCommand<DragEventArgs>(async e => await DragDrop(e));
            ImportCommand = new RelayCommand(async () => await ImportDownloadLinks());
            ExportCommand = new RelayCommand(() => ExportDownloadLinks());
            ExitCommand = new RelayCommand(() => Environment.Exit(0));
            ReloadAllCommand = new RelayCommand(async () => await ReloadAll(), () => VideoInfos.Count > 0);
            DownloadAllCommand = new RelayCommand(async () => await DownloadAll(), () => VideoInfos.Count > 0);
            CancelAllCommand = new RelayCommand(() => CancelAllDownloads(), () => InProcessCount > 0);
            RemoveUnavailableCommand = new RelayCommand(() => RemoveAllUnavailable());
            RemoveFinishedCommand = new RelayCommand(() => RemoveAllFinished());
            DeleteCommand = new RelayCommand<ItemViewModel<T>>(o => Delete(o));
            SmartAutomationCommand = new RelayCommand(() => SmartAutomationMode = !SmartAutomationMode);
            SettingsCommand = new RelayCommand<int>(
                (p) => Messenger.Default.Send(new ShowWindowMessage(WindowType.SettingsWindow, p, callback: applySettingsCallback))
            );
            OpenDownloadFolderCommand = new RelayCommand(
                () => fileService.ShowInExplorer(Settings.Default.DownloadFolder, true)
            );
            SelectDownloadFolderCommand = new RelayCommand(() => SelectDownloadFolder(), true);
            ShowDownloadOutputWindowCommand = new RelayCommand(
                () => Messenger.Default.Send(new ShowWindowMessage(WindowType.DownloadOutputWindow))
            );
            OpenErrorLogCommand = new RelayCommand(() => OpenErrorLog());
            CheckForUpdatesCommand = new RelayCommand(async () => await CheckForUpdates());
            AboutCommand = new RelayCommand(() => ShowAboutBox());
        }

        private bool checkExists(string url)
            => VideoInfos.Any(v => v.Entry?.Url == url);

        public async Task FetchVideo(string videoUrl)
        {
            // Check for duplicates
            if (!Settings.Default.AllowDuplicateEntries && checkExists(videoUrl))
            {
                dialogService.ShowMessage(Resources.MainWindow_AlreadyFetched, Resources.Info);
                return;
            }
            await itemProvider.FetchItemList(new[] { videoUrl }, VideoInfos, this, dialogService);
        }

        public async Task DragDrop(DragEventArgs e)
        {
            // Check if dropped item is a string
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string videoUrl = (string)e.Data.GetData(DataFormats.StringFormat);
                await FetchVideo(videoUrl);
            }
        }

        private async void fetchCommandCallback(bool? dialogResult, object param)
        {
            if (dialogResult.GetValueOrDefault())
            {
                FetchViewModel viewModel = param as FetchViewModel;
                var fetchedVideos = await itemProvider.FetchItemList(
                    viewModel.VideoUrls, VideoInfos, this, dialogService,
                    selectedFormat: viewModel.SelectedDownloadOption, overrideOptions: viewModel.OverrideOptions
                );
                if (viewModel.ImmediateDownload)
                {
                    var tasks = new List<Task>();
                    foreach (var vid in fetchedVideos)
                    {
                        tasks.Add(vid.DownloadVideo());
                    }
                    await Task.WhenAll(tasks.ToArray());
                }
            }
        }

        private async void automationService_InputReceived(object sender, UrlsEventArgs e)
        {
            var fetchedVideos = await itemProvider.FetchItemList(e.Urls, VideoInfos, this, dialogService);
            if (Settings.Default.SmartAutomationDownload)
            {
                var tasks = fetchedVideos.Select(vid => vid.DownloadVideo());
                await Task.WhenAll(tasks);
            }
        }

        public async Task ImportDownloadLinks()
        {
            string path = fileService.SelectOpenFile("Text files (*.txt)|*.txt");
            if (!String.IsNullOrEmpty(path))
            {
                var lines = File.ReadAllLines(path)
                                .Select((s) => s.Trim())
                                .Where((s) => !s.StartsWith("#"));
                // Remove duplicates if neccessary
                if (!Settings.Default.AllowDuplicateEntries)
                    lines = lines.Distinct()
                                .Where(url => !checkExists(url));
                await itemProvider.FetchItemList(lines.ToArray(), VideoInfos, this, dialogService);
            }
        }

        public void ExportDownloadLinks()
        {
            string path = fileService.SelectSaveFile("Text files (*.txt)|*.txt");
            if (!String.IsNullOrEmpty(path))
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    foreach (var vid in VideoInfos)
                        sw.WriteLine(vid.Entry.Url);
                }
            }
        }

        public async Task ReloadAll()
        {
            var tasks = new List<Task>();
            foreach (var vid in VideoInfos)
            {
                tasks.Add(vid.Reload());
            }
            await Task.WhenAll(tasks.ToArray());
        }

        public async Task DownloadAll()
        {
            var tasks = new List<Task>();
            foreach (var vid in VideoInfos)
            {
                tasks.Add(vid.DownloadVideo());
            }
            await Task.WhenAll(tasks.ToArray());
        }

        public void CancelAllDownloads()
        {
            foreach (var vid in VideoInfos)
            {
                vid.Entry?.CancelDownload();
            }
        }

        public void RemoveAllUnavailable()
        {
            VideoInfos = new ObservableCollection<ItemViewModel<T>>(VideoInfos.Where(v => !v.Unavailable));
        }

        public void RemoveAllFinished()
        {
            VideoInfos = new ObservableCollection<ItemViewModel<T>>(VideoInfos.Where(v => v.State != ItemState.Succeeded));
        }

        public void SetStats(int count = 1, bool finished = false, bool success = true)
        {
            if (!finished)
            {
                InProcessCount += count;
            }
            else
            {
                InProcessCount -= count;
                if (success) SuccessCount += count;
                else FailedCount += count;
            }
        }

        public void Delete(ItemViewModel<T> item)
        {
            VideoInfos.Remove(item);
        }

        public void Clear()
        {
            VideoInfos.Clear();
            InProcessCount = 0;
            SuccessCount = 0;
            FailedCount = 0;
        }

        public void SelectDownloadFolder()
        {
            string dir = fileService.SelectFolder(
                Resources.SelectDirectory_Description,
                Settings.Default.DownloadFolder);
            if (!String.IsNullOrWhiteSpace(dir))
            {
                Settings.Default.DownloadFolder = dir;
                App.InitializeDownloadEngine();
            }
        }

        public void OpenErrorLog()
        {
            if (!App.OpenErrorLog()) dialogService.ShowMessageBox("No error log available.", "Vividl - " + Resources.Info);
        }

        public async Task CheckForUpdates()
        {
            var ytdlUpdateService = SimpleIoc.Default.GetInstance<YtdlUpdateService>();
            var msg = await ytdlUpdateService.Update();
            dialogService.ShowMessageBox(msg, "Vividl - " + Resources.Info);
            var updateChecker = SimpleIoc.Default.GetInstance<IUpdateChecker>();
            updateChecker.CheckForUpdates(UpdateNotifyMode.Always);
        }

        public void ShowAboutBox()
        {
            var updateChecker = SimpleIoc.Default.GetInstance<IUpdateChecker>();
            dialogService.ShowAboutBox(updateChecker);
        }

        private void applySettingsCallback(bool? dialogResult, object param)
        {
            if (dialogResult.GetValueOrDefault())
            {
                foreach (var item in VideoInfos)
                {
                    item.UpdateCurrentFormats();
                }
            }
        }
    }
}
