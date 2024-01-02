using System;
using System.ComponentModel;
using System.Windows;
using AdonisUI.Controls;

namespace Vividl.View
{
    public partial class NameEditWindow : AdonisWindow, IDataErrorInfo
    {
        public string NameValue { get; set; }

        public string Error => this[nameof(NameValue)];

        public string this[string name]
        {
            get
            {
                if (name == nameof(NameValue))
                {
                    if (String.IsNullOrWhiteSpace(NameValue))
                        return Vividl.Properties.Resources.Validation_ValueNonEmpty;
                }
                return null;
            }
        }

        public NameEditWindow(string name)
        {
            InitializeComponent();
            this.DataContext = this;
            this.NameValue = name;
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(Error))
            {
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
