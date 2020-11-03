using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Bluegrams.Application;
using Bluegrams.Application.WPF;
using Enterwell.Clients.Wpf.Notifications;
using GalaSoft.MvvmLight.Ioc;
using MahApps.Metro.IconPacks;
using Vividl.Properties;
using WPFLocalizeExtension.Engine;

using Brushes = AdonisUI.Brushes;
using Colors = AdonisUI.Colors;

namespace Vividl.Services
{
    class NotificationDialogService : SimpleDialogService
    {
        private INotificationMessageManager notificationManager;

        public NotificationDialogService()
        {
            AccentColor = App.DARK_ACCENT;
            notificationManager = SimpleIoc.Default.GetInstance<INotificationMessageManager>();
        }

        public override void ShowAboutBox(IUpdateChecker updateChecker = null)
        {
            Uri iconUri = new Uri("pack://application:,,,/vividl.ico");
            AboutBox aboutBox = new AboutBox(new BitmapImage(iconUri));
            aboutBox.UpdateChecker = updateChecker;
            aboutBox.AccentColor = AccentColor;
            aboutBox.CultureChanging += (o, args) =>
            {
                LocalizeDictionary.Instance.Culture = args.NewCulture;
                args.Success = true;
            };
            aboutBox.Owner = Application.Current.Windows
                                .OfType<Window>().SingleOrDefault(x => x.IsActive);
            aboutBox.ShowDialog();
        }

        public override void ShowError(string message, string title, string buttonText = null)
            => createMessage(message, title, buttonText, PackIconModernKind.Stop);

        public override void ShowMessage(string message, string title, string buttonText = null)
            => createMessage(message, title, buttonText, PackIconModernKind.InformationCircle);

        public override void ShowWarning(string message, string title, string buttonText = null)
            => createMessage(message, title, buttonText, PackIconModernKind.Warning);

        private void createMessage(string message, string title, string buttonText, PackIconModernKind icon)
        {
            notificationManager.CreateMessage()
                .Accent(new SolidColorBrush(AccentColor))
                .Background((Brush)Application.Current.Resources[Brushes.Layer3BackgroundBrush])
                .Foreground(((Color)Application.Current.Resources[Colors.ForegroundColor]).ToString())
                .HasMessage($"{title}:\n{message}")
                .WithAdditionalContent(ContentLocation.AboveBadge, new PackIconModern()
                {
                    Kind = icon,
                    Height = 18,
                    Width = 18
                })
                .Dismiss().WithButton(buttonText ?? Resources.Submit, button => { })
                .Dismiss().WithDelay(TimeSpan.FromSeconds(7)).Queue();
        }
    }
}
