using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Bluegrams.Application;
using Bluegrams.Application.WPF;
using Enterwell.Clients.Wpf.Notifications;
using Enterwell.Clients.Wpf.Notifications.Controls;
using MahApps.Metro.IconPacks;
using Vividl.Properties;
using WPFLocalizeExtension.Engine;

using Brushes = AdonisUI.Brushes;
using Colors = AdonisUI.Colors;

namespace Vividl.Services
{
    public class NotificationDialogService : SimpleDialogService
    {
        public INotificationMessageManager MainMessageManager { get; }

        public event EventHandler<NotificationEventArgs> NotificationAdded;

        public event EventHandler<NotificationEventArgs> NotificationRemoved;

        public NotificationDialogService()
        {
            AccentColor = App.DARK_ACCENT;
            // Show a maximum of 3 notifications.
            MainMessageManager = new CappedNotificationMessageManager(3);
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

        private NotificationMessage createNotificationMsgObject(string message, string title, string buttonText, PackIconModernKind icon)
        {
            return new NotificationMessage
            {
                AccentBrush = new SolidColorBrush(AccentColor),
                Background = (Brush)Application.Current.Resources[Brushes.Layer3BackgroundBrush],
                Foreground = new SolidColorBrush((Color)Application.Current.Resources[Colors.ForegroundColor]),
                Message = $"{title}:\n{message}",
                AdditionalContentOverBadge = new PackIconModern()
                {
                    Kind = icon,
                    Height = 18,
                    Width = 18
                },
                Buttons = new ObservableCollection<object>
                {
                   new NotificationMessageButton()
                   {
                       Content = Resources.Copy,
                       Callback = button => Clipboard.SetText($"{title}:\n{message}")
                   }
                }
            };
        }

        private void createMessage(string message, string title, string buttonText, PackIconModernKind icon)
        {
            // Add to main manager
            var builder = MainMessageManager.CreateMessage();
            builder.Manager = MainMessageManager;
            builder.Message = createNotificationMsgObject(message, title, buttonText, icon);
            builder.Dismiss().WithButton(buttonText ?? Resources.Submit, button => { })
                .Dismiss().WithDelay(TimeSpan.FromSeconds(7)).Queue();
            // Add to log
            var msgObject = createNotificationMsgObject(message, title, buttonText, icon);
            msgObject.Buttons.Add(
                new NotificationMessageButton()
                {
                    Content = buttonText ?? Resources.Submit,
                    Callback = button => NotificationRemoved?.Invoke(this, new NotificationEventArgs(msgObject)),
                }
            );
            NotificationAdded?.Invoke(this, new NotificationEventArgs(msgObject));
        }
    }

    public class NotificationEventArgs : EventArgs
    {
        public NotificationMessage Message { get; }

        public NotificationEventArgs(NotificationMessage message)
        {
            Message = message;
        }
    }
}
