using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using AdonisUI.Controls;
using Vividl.ViewModel;

namespace Vividl.View
{
    partial class VideoDataWindow : AdonisWindow
    {
        public VideoDataWindow(VideoViewModel video)
        {
            InitializeComponent();
            this.DataContext = video;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
