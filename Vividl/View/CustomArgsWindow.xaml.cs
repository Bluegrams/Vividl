using System;
using System.Windows;
using AdonisUI.Controls;

namespace Vividl.View
{
    public partial class CustomArgsWindow : AdonisWindow
    {
        public CustomArgsWindow(string args)
        {
            InitializeComponent();
            txtArgs.Text = args;
        }

        public object ReturnValue { get; set; }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.ReturnValue = txtArgs.Text;
            this.Close();
        }
    }
}
