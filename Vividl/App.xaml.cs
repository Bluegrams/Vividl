using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Bluegrams.Application;
using GalaSoft.MvvmLight.Ioc;
using Vividl.Model;
using Vividl.Properties;
using Vividl.Services;
using Vividl.Services.Update;
using Vividl.ViewModel;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

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

        public static bool OpenErrorLog()
        {
            string logFile = Path.Combine(Path.GetTempPath(), AppInfo.ProductName.ToLower() + ".log");
            if (File.Exists(logFile))
            {
                Process.Start(logFile);
                return true;
            }
            return false;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException; ;
            // register services and view models
            registerServices();
            registerVMs();
            // setup main window
            MainWindow mainWindow = new MainWindow();
            // main window might restore settings from upgrade -> apply settings after
            InitializeDefaultSettings();
            InitializeDownloadEngine();
            // apply theme
            var themeResolver = SimpleIoc.Default.GetInstance<IThemeResolver>();
            themeResolver.SetColorScheme(Settings.Default.AppTheme);
            var mainVm = SimpleIoc.Default.GetInstance<MainViewModel<MediaEntry>>();
            mainWindow.DataContext = mainVm;
            mainWindow.Loaded += async (o, _) => await mainVm.Initialize();
            SimpleIoc.Default.Register<IUpdateChecker>(
                () => new WpfUpdateChecker(UPDATE_URL, mainWindow, UPDATE_IDENTIFIER));
            mainWindow.Show();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Default.Log("An unhandled exception caused the application to terminate unexpectedly.",
                (Exception)e.ExceptionObject);
            // Try to save current state
            try
            {
                Deinitialize();
                MessageBox.Show(Vividl.Properties.Resources.AppCrashWindow_Text, Vividl.Properties.Resources.AppCrashWindow_Title);
            }
            catch (Exception) { }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
            => Deinitialize();

        public void Deinitialize()
        {
            var mainVm = SimpleIoc.Default.GetInstance<MainViewModel<MediaEntry>>();
            mainVm.Deinitialize();
            Settings.Default.Save();
        }

        public static void InitializeDefaultSettings()
        {
            // Set a default download folder if none is specified.
            if (String.IsNullOrEmpty(Settings.Default.DownloadFolder))
                Settings.Default.DownloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos) + "\\Downloads";
#if (WITH_LIB && PORTABLE)
            if (String.IsNullOrEmpty(Settings.Default.YoutubeDLPath) ||
                // switch from youtube-dl.exe to yt-dlp.exe
                isOldLibDefaultPath(Settings.Default.YoutubeDLPath, "youtube-dl.exe")
            )
                Settings.Default.YoutubeDLPath = Path.Combine("Lib", "yt-dlp.exe");
            if (String.IsNullOrEmpty(Settings.Default.FfmpegPath))
                Settings.Default.FfmpegPath = Path.Combine("Lib", "ffmpeg.exe");
            if (String.IsNullOrEmpty(Settings.Default.JSRuntimePath))
                Settings.Default.JSRuntimePath = "quickjs:" + Path.Combine("Lib", "qjs.exe");
#elif WITH_LIB
            string libPathBase = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Bluegrams", "Vividl", "Lib");
            if (String.IsNullOrEmpty(Settings.Default.YoutubeDLPath) ||
                // move old location to new location
                isOldLibDefaultPath(Settings.Default.YoutubeDLPath, "youtube-dl.exe") ||
                // switch from youtube-dl.exe to yt-dlp.exe
                Settings.Default.YoutubeDLPath == Path.Combine(libPathBase, "youtube-dl.exe")
            )
            {
                Settings.Default.YoutubeDLPath = Path.Combine(libPathBase, "yt-dlp.exe");
            }
            if (String.IsNullOrEmpty(Settings.Default.FfmpegPath) ||
                // move old location to new location
                isOldLibDefaultPath(Settings.Default.FfmpegPath, "ffmpeg.exe")
            )
            {
                Settings.Default.FfmpegPath = Path.Combine(libPathBase, "ffmpeg.exe");
            }
            if (String.IsNullOrEmpty(Settings.Default.JSRuntimePath)
            )
            {
                Settings.Default.JSRuntimePath = "quickjs:" + Path.Combine(libPathBase, "qjs.exe");
            }
#else
            if (String.IsNullOrEmpty(Settings.Default.YoutubeDLPath))
                Settings.Default.YoutubeDLPath = "yt-dlp.exe";
            if (String.IsNullOrEmpty(Settings.Default.FfmpegPath))
                Settings.Default.FfmpegPath = "ffmpeg.exe";
            if (String.IsNullOrEmpty(Settings.Default.JSRuntimePath))
                Settings.Default.JSRuntimePath = "deno";
#endif
        }

        private static bool isOldLibDefaultPath(string path, string executable)
        {
            string oldDefaultPath = Path.Combine(Path.GetDirectoryName(AppInfo.Location), "Lib", executable);
            return path == oldDefaultPath;
        }

        public static void InitializeDownloadEngine()
        {
            var ytdl = SimpleIoc.Default.GetInstance<CustomYoutubeDL>();
            ytdl.YoutubeDLPath = Settings.Default.YoutubeDLPath;
            ytdl.FFmpegPath = Settings.Default.FfmpegPath;
            ytdl.OutputFolder = Settings.Default.DownloadFolder;
            ytdl.OutputFileTemplate = "%(title)s.%(ext)s";
            ytdl.RestrictFilenames = Settings.Default.RestrictFilenames;
            ytdl.OverwriteFiles = Settings.Default.OverwriteMode == OverwriteMode.Overwrite;
            ytdl.AddMetadata = Settings.Default.AddMetadata;
            ytdl.Proxy = Settings.Default.Proxy;
            ytdl.FormatSort = Settings.Default.DefaultResolution.ToFormatSort();
            ytdl.JSRuntimePath = Settings.Default.JSRuntimePath;
            if (Settings.Default.UseArchive)
            {
                ytdl.DownloadArchive = Path.Combine(Settings.Default.DownloadFolder, Settings.Default.ArchiveFilename);
            }
            else ytdl.DownloadArchive = null;
            if (!String.IsNullOrEmpty(Settings.Default.CustomDownloaderArgs))
            {
                ytdl.CustomDownloadOptions = OptionSet.FromString(
                    Settings.Default.CustomDownloaderArgs.Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                );
            }
            else
            {
                ytdl.CustomDownloadOptions = null;
            }
        }

        // Dumb way to determine if we are likely using yt-dlp
        public static bool UsingYtDlp => Settings.Default.YoutubeDLPath.Contains("yt-dlp");

        private void registerServices()
        {
            SimpleIoc.Default.Register<IDownloadOptionProvider, VideoDownloadOptionProvider>();
            SimpleIoc.Default.Register<IItemProvider<MediaEntry>, VividlItemProvider>();
            SimpleIoc.Default.Register<IFileService, FileService>();
            SimpleIoc.Default.Register<NotificationDialogService>();
            SimpleIoc.Default.Register<IDialogService>(() => SimpleIoc.Default.GetInstance<NotificationDialogService>());
            SimpleIoc.Default.Register<IThemeResolver, ThemeResolver>();
            SimpleIoc.Default.Register(() => new CustomYoutubeDL(Settings.Default.MaxProcesses));
            SimpleIoc.Default.Register<YoutubeDL>(() => SimpleIoc.Default.GetInstance<CustomYoutubeDL>());
            SimpleIoc.Default.Register<YtdlUpdateService>();
            SimpleIoc.Default.Register<FFmpegUpdateService>();
            SimpleIoc.Default.Register<SmartAutomationService>();
        }

        private void registerVMs()
        {
            SimpleIoc.Default.Register<MainViewModel<MediaEntry>>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<NotificationViewModel>();
        }
    }
}
