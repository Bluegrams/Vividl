﻿using System;
using System.Globalization;
using System.Windows;
using AdonisUI.Controls;
using Bluegrams.Application;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Vividl.Properties;
using Vividl.View;
using Vividl.ViewModel;
using WPFLocalizeExtension.Engine;

namespace Vividl
{
    public partial class MainWindow : AdonisWindow
    {
        public MainWindow()
        {
            var manager = new WpfWindowManager(this);
            manager.ManageDefault();
            manager.ApplyToSettings(Settings.Default);
            manager.Initialize();
            InitializeComponent();
            Messenger.Default.Register<NotificationMessage>(this, handleStatusMessage);
            Messenger.Default.Register<ShowWindowMessage>(this, handleOpenWindow);
            this.Closing += MainWindow_Closing;
            var culture = new CultureInfo(Bluegrams.Application.Properties.Settings.Default.Culture);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            LocalizeDictionary.Instance.Culture = CultureInfo.CurrentUICulture;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.AutoCheckUpdates)
            {
                var updateChecker = SimpleIoc.Default.GetInstance<IUpdateChecker>();
                updateChecker.CheckForUpdates(UpdateNotifyMode.Auto);
            }
            // Check for Youtube-DL updates
            if (Settings.Default.AutoCheckUpdatesYoutubeDL)
            {
                var settingsVm = SimpleIoc.Default.GetInstance<SettingsViewModel>();
                await settingsVm.CheckForYoutubeDLUpdates();
            }
        }

        private void handleStatusMessage(NotificationMessage msg)
        {
            txtStatus.Text = msg.Notification;
        }

        private void handleOpenWindow(ShowWindowMessage msg)
        {
            bool? dialogResult = null;
            object returnVal = null;
            switch (msg.Window)
            {
                case WindowType.SettingsWindow:
                    var settingsWindow = new SettingsWindow((int)msg.Parameter);
                    settingsWindow.Owner = this;
                    dialogResult = settingsWindow.ShowDialog();
                    break;
                case WindowType.FetchWindow:
                    var fetchWindow = new FetchWindow(msg.Parameter as FetchViewModel);
                    fetchWindow.Owner = this;
                    dialogResult = fetchWindow.ShowDialog();
                    returnVal = msg.Parameter;
                    break;
                case WindowType.DownloadOutputWindow:
                    DownloadOutputWindow.ShowDownloadOutputWindow(this);
                    break;
                case WindowType.NotificationLogWindow:
                    NotificationLogWindow.ShowNotificationLogWindow(SimpleIoc.Default.GetInstance<NotificationViewModel>(), this);
                    break;
                case WindowType.NameEditWindow:
                    var editWindow = new NameEditWindow((string)msg.Parameter);
                    editWindow.Owner = this;
                    dialogResult = editWindow.ShowDialog();
                    returnVal = editWindow.NameValue;
                    break;
#if VIVIDL
                case WindowType.FormatSelectionWindow:
                    var formatSelectionWindow = new FormatSelectionWindow(msg.Parameter as FormatSelectionViewModel);
                    formatSelectionWindow.Owner = this;
                    dialogResult = formatSelectionWindow.ShowDialog();
                    break;
                case WindowType.VideoDataWindow:
                    var videoDataWindow = new VideoDataWindow(msg.Parameter as VideoViewModel);
                    videoDataWindow.Owner = this;
                    dialogResult = videoDataWindow.ShowDialog();
                    break;
                case WindowType.PlaylistDataWindow:
                    var playlistDataWindow = new PlaylistDataWindow(msg.Parameter as VideoViewModel);
                    playlistDataWindow.Owner = this;
                    dialogResult = playlistDataWindow.ShowDialog();
                    break;
                case WindowType.CustomArgsWindow:
                    var customArgsWindow = new CustomArgsWindow(msg.Parameter as string);
                    customArgsWindow.Owner = this;
                    dialogResult = customArgsWindow.ShowDialog();
                    returnVal = customArgsWindow.ReturnValue;
                    break;
#else
                case WindowType.VideoDataWindow:
                case WindowType.PlaylistDataWindow:
                    break;
#endif
            }
            msg.Callback?.Invoke(dialogResult, returnVal);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Messenger.Default.Unregister(this);
        }
    }
}
