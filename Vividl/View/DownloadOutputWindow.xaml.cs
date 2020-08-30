using System;
using System.Windows;
using AdonisUI.Controls;
using Vividl.Services;

namespace Vividl.View
{
    public partial class DownloadOutputWindow : AdonisWindow
    {
        private static DownloadOutputWindow instance = null;

        public static void ShowDownloadOutputWindow(Window owner = null)
        {
            if (instance != null)
            {
                if (instance.WindowState == WindowState.Minimized)
                    instance.WindowState = WindowState.Normal;
                instance.Activate();
            }
            else
            {
                instance = new DownloadOutputWindow();
                instance.Owner = owner;
                instance.Show();
            }
        }

        private DownloadOutputWindow()
        {
            InitializeComponent();
            DownloadOutputLogger.Instance.OutputReceived += (o, e) =>
            {
                txtOutput.Text += $"{Environment.NewLine}[{e.JobId}] {e.Output}";
                txtOutput.ScrollToEnd();
            };
        }

        private void CopyOutputMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(txtOutput.Text);
        }

        private void ClearOutputMenuItem_Click(object sender, RoutedEventArgs e)
        {
            txtOutput.Clear();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            instance = null;
        }
    }
}
