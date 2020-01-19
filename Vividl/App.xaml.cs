using System;
using System.Windows;
using System.Windows.Media;
using Bluegrams.Application;
using Enterwell.Clients.Wpf.Notifications;
using GalaSoft.MvvmLight.Ioc;
using Vividl.Model;
using Vividl.Properties;
using Vividl.Services;
using Vividl.ViewModel;
using YoutubeDLSharp;

namespace Vividl
{
    public partial class App : Application
    {
        // Accent colors
        public static readonly Color LIGHT_ACCENT = Color.FromRgb(204, 102, 119);
        public static readonly Color DARK_ACCENT = Color.FromRgb(168, 36, 58);

        private const string UPDATE_URL = "https://vividl.sourceforge.io/update.xml";
#if PORTABLE
        private const string UPDATE_IDENTIFIER = "portable";
#else
        private const string UPDATE_IDENTIFIER = "install";
#endif

        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if PORTABLE
            PortableSettingsProvider.ApplyProvider(Settings.Default);
#endif
            // Set a default download folder if none is specified.
            if (String.IsNullOrEmpty(Settings.Default.DownloadFolder))
                Settings.Default.DownloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) + "\\Downloads";
            // register services and view models
            registerServices();
            InitializeDownloadEngine();
            registerVMs();
            // apply theme
            var themeResolver = SimpleIoc.Default.GetInstance<IThemeResolver>();
            themeResolver.SetColorScheme(Settings.Default.AppTheme);
            // setup main window
            MainWindow mainWindow = new MainWindow();
            mainWindow.DataContext = SimpleIoc.Default.GetInstance<MainViewModel<MediaEntry>>();
            SimpleIoc.Default.Register<IUpdateChecker>(
                () => new WpfUpdateChecker(UPDATE_URL, mainWindow, UPDATE_IDENTIFIER));
            mainWindow.Show();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.Default.Save();
        }

        public static void InitializeDownloadEngine()
        {
            var ytdl = SimpleIoc.Default.GetInstance<YoutubeDL>();
            ytdl.YoutubeDLPath = Settings.Default.YoutubeDLPath;
            ytdl.FFmpegPath = Settings.Default.FfmpegPath;
            ytdl.OutputFolder = Settings.Default.DownloadFolder;
            ytdl.RestrictFilenames = Settings.Default.RestrictFilenames;
            ytdl.OverwriteFiles = Settings.Default.OverwriteFiles;
        }

        private void registerServices()
        {
            SimpleIoc.Default.Register<IDownloadOptionProvider, VideoDownloadOptionProvider>();
            SimpleIoc.Default.Register<IItemProvider<MediaEntry>, VividlItemProvider>();
            SimpleIoc.Default.Register<IFileService, FileService>();
            SimpleIoc.Default.Register<IDialogService, NotificationDialogService>();
            SimpleIoc.Default.Register<INotificationMessageManager, NotificationMessageManager>();
            SimpleIoc.Default.Register<IThemeResolver, ThemeResolver>();
            SimpleIoc.Default.Register(() => new YoutubeDL(Settings.Default.MaxProcesses)); 
        }

        private void registerVMs()
        {
            SimpleIoc.Default.Register<MainViewModel<MediaEntry>>();
            SimpleIoc.Default.Register<SettingsViewModel>();
        }
    }
}
