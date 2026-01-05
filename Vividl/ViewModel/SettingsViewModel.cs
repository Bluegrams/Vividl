using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Bluegrams.Application;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Vividl.Model;
using Vividl.Properties;
using Vividl.Services;
using Vividl.Services.Update;

namespace Vividl.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {
        private IDialogService dialogService;
        private IThemeResolver themeResolver;
        private IUpdateChecker updateChecker;

        public IFileService FileService { get; }

        public Settings Settings => Settings.Default;

        public ObservableCollection<IDownloadOption> DefaultFormats { get; }

        public ICommand SwitchAppThemeCommand { get; }
        public ICommand EditCustomArgsCommand { get; }

        public ILibUpdateService YoutubeDLUpdateService { get; }
        public ILibUpdateService FFmpegUpdateService { get; }

        public ICommand UpdateVividlCommand { get; }
        public ICommand UpdateYoutubeDLCommand { get; }
        public ICommand UpdateFFmpegCommand { get; }

        public ICommand ResetAllSettingsCommand { get; }

        public SettingsViewModel(IFileService fileService, IDialogService dialogService,
            IThemeResolver themeResolver, IDownloadOptionProvider optionProvider,
            IUpdateChecker updateChecker, YtdlUpdateService ytdlUpdateService, FFmpegUpdateService ffmpegUpdateService)
        {
            this.FileService = fileService;
            this.dialogService = dialogService;
            this.themeResolver = themeResolver;
            this.updateChecker = updateChecker;
            this.YoutubeDLUpdateService = ytdlUpdateService;
            this.FFmpegUpdateService = ffmpegUpdateService;
            DefaultFormats = new ObservableCollection<IDownloadOption>(optionProvider.CreateDownloadOptions());
            SwitchAppThemeCommand = new RelayCommand(() => SwitchAppTheme());
            EditCustomArgsCommand = new RelayCommand(
                () => Messenger.Default.Send(
                    new ShowWindowMessage(WindowType.CustomArgsWindow, parameter: Settings.Default.CustomDownloaderArgs, callback: editCustomArgsCallback)
                )
            );
            UpdateVividlCommand = new RelayCommand(() => UpdateVividl());
            UpdateYoutubeDLCommand = new RelayCommand(async () => await CheckForYoutubeDLUpdates());
            UpdateFFmpegCommand = new RelayCommand(async () => await CheckForFFmpegUpdates());
            ResetAllSettingsCommand = new RelayCommand(() => ResetAllSettings());
        }

        private void editCustomArgsCallback(bool? dialogResult, object param)
        {
            if (dialogResult.GetValueOrDefault())
            {
                Settings.Default.CustomDownloaderArgs = param.ToString();
            }
        }

        public void SwitchAppTheme()
            => themeResolver.SetColorScheme(Settings.Default.AppTheme);

        public void UpdateVividl() => updateChecker.CheckForUpdates(UpdateNotifyMode.Always);

        public async Task UpdateYoutubeDL()
        {
            var msg = await YoutubeDLUpdateService.Update();
            dialogService.ShowMessageBox(msg, "Vividl - " + Resources.Info);
        }

        public async Task CheckForYoutubeDLUpdates()
        {
            bool willUpdate = await YoutubeDLUpdateService.CheckForUpdates();
            if (willUpdate)
            {
                await UpdateYoutubeDL();
            }
        }

        public async Task UpdateFFmpeg()
        {
            var msg = await FFmpegUpdateService.Update();
            dialogService.ShowMessageBox(msg, "Vividl - " + Resources.Info);
        }

        public async Task CheckForFFmpegUpdates()
        {
            bool willUpdate = await FFmpegUpdateService.CheckForUpdates();
            if (willUpdate)
            {
                await UpdateFFmpeg();
            }
        }

        public async Task ApplySettings()
        {
            App.InitializeDownloadEngine();
            var ydl = SimpleIoc.Default.GetInstance<YoutubeDLSharp.YoutubeDL>();
            await ydl.SetMaxNumberOfProcesses(Settings.Default.MaxProcesses);
        }

        public void ResetAllSettings()
        {
            if (dialogService.ShowConfirmation(Resources.SettingsWindow_ConfirmReset, "Vividl - " + Resources.Warning))
                Settings.Default.Reset();
        }
    }
}
