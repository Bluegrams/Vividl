using System;
using System.Windows;
using AdonisUI.Controls;
using Vividl.ViewModel;

namespace Vividl.View
{
    public partial class NotificationLogWindow : AdonisWindow
    {
        private static NotificationLogWindow instance = null;

        public static void ShowNotificationLogWindow(NotificationViewModel notificationVm, Window owner = null)
        {
            if (instance != null)
            {
                if (instance.WindowState == WindowState.Minimized)
                    instance.WindowState = WindowState.Normal;
                instance.Activate();
            }
            else
            {
                instance = new NotificationLogWindow(notificationVm);
                instance.Owner = owner;
                instance.Show();
            }
        }

        private NotificationLogWindow(NotificationViewModel notificationVm)
        {
            InitializeComponent();
            this.DataContext = notificationVm;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            instance = null;
        }
    }
}
