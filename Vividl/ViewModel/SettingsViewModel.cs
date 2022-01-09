using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Bluegrams.Application;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using Vividl.Model;
using Vividl.Properties;
using Vividl.Services;

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

        public ILibUpdateService YoutubeDLUpdateService { get; }
        public ICommand UpdateVividlCommand { get; }
        public ICommand UpdateYoutubeDLCommand { get; }

        public SettingsViewModel(IFileService fileService, IDialogService dialogService,
            IThemeResolver themeResolver, IDownloadOptionProvider optionProvider,
            IUpdateChecker updateChecker, YtDlpUpdateService ytdlUpdateService)
        {
            this.FileService = fileService;
            this.dialogService = dialogService;
            this.themeResolver = themeResolver;
            this.updateChecker = updateChecker;
            this.YoutubeDLUpdateService = ytdlUpdateService;
            DefaultFormats = new ObservableCollection<IDownloadOption>(optionProvider.CreateDownloadOptions());
            SwitchAppThemeCommand = new RelayCommand(() => SwitchAppTheme());
            UpdateVividlCommand = new RelayCommand(() => UpdateVividl());
            UpdateYoutubeDLCommand = new RelayCommand(async () => await UpdateYoutubeDL());
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

        public async Task ApplySettings()
        {
            App.InitializeDownloadEngine();
            var ydl = SimpleIoc.Default.GetInstance<YoutubeDLSharp.YoutubeDL>();
            await ydl.SetMaxNumberOfProcesses(Settings.Default.MaxProcesses);
        }
    }
}
