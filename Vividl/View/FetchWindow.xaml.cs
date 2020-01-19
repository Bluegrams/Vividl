using System;
using System.Windows;
using AdonisUI.Controls;

namespace Vividl.View
{
    public partial class FetchWindow : AdonisWindow
    {
        public string[] VideoUrls { get; private set; }

        public FetchWindow()
        {
            InitializeComponent();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            this.VideoUrls = txtUrl.Text.Split(
                                new[] { Environment.NewLine },
                                StringSplitOptions.RemoveEmptyEntries);
            this.DialogResult = true;
            this.Close();
        }
    }
}
