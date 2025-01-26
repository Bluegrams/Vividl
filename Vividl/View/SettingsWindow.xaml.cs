using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using AdonisUI.Controls;
using GalaSoft.MvvmLight.Ioc;
using Vividl.ViewModel;

namespace Vividl.View
{
    public partial class SettingsWindow : AdonisWindow
    {
        SettingsViewModel vm;
        HashSet<BindingExpressionBase> beSet;

        public SettingsWindow(int initialPage = 0)
        {
            InitializeComponent();
            this.vm = SimpleIoc.Default.GetInstance<SettingsViewModel>();
            this.DataContext = vm;
            beSet = new HashSet<BindingExpressionBase>();
            tabSettings.SelectedIndex = initialPage;
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Passing 'null' returns all binding expression that haven't been updated.
            beSet.UnionWith(BindingOperations.GetSourceUpdatingBindings(null));
            foreach (var be in beSet)
            {
                be.UpdateSource();
            }
            await vm.ApplySettings();
            this.DialogResult = true;
            this.Close();
        }

        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            beSet.UnionWith(BindingOperations.GetSourceUpdatingBindings(this));
        }
    }
}
