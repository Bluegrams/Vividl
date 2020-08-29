using System;
using System.Windows;
using AdonisUI.Controls;
using Vividl.ViewModel;

namespace Vividl.View
{
    public partial class FetchWindow : AdonisWindow
    {
        private FetchViewModel viewModel;

        public FetchWindow(FetchViewModel viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            this.DataContext = viewModel;
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(txtUrl.Text))
            {
                if (!String.IsNullOrWhiteSpace(txtPassword.Password))
                    viewModel.OverrideOptions.Password = txtPassword.Password;
                if (!String.IsNullOrWhiteSpace(txtVideoPassword.Password))
                    viewModel.OverrideOptions.VideoPassword = txtVideoPassword.Password;
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
