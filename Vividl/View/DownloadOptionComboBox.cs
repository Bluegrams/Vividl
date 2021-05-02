using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Vividl.Model;

namespace Vividl.View
{
    class DownloadOptionComboBox : ComboBox
    {
        public static readonly DependencyProperty CustomDownloadCommandProperty =
            DependencyProperty.Register("CustomDownloadCommand",
                typeof(ICommand), typeof(DownloadOptionComboBox)
            );

        public ICommand CustomDownloadCommand
        {
            get => (ICommand)GetValue(CustomDownloadCommandProperty);
            set => SetValue(CustomDownloadCommandProperty, value);
        }

        public event EventHandler CustomDownloadSelected;

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            // Only trigger event & command if selection change was done by user.
            // We try to ensure this via "IsDropDownOpen || IsKeyboardFocused".
            if (ItemsSource is DownloadOptionCollection downloadOptions
                && (IsDropDownOpen || IsKeyboardFocused))
            {
                if (e.AddedItems[0] is CustomDownload)
                {
                    CustomDownloadSelected?.Invoke(this, EventArgs.Empty);
                    CustomDownloadCommand?.Execute(
                        new Action<bool>((applied) =>
                            {
                                // Reset selection if custom download was not configured.
                                // This avoids invalid empty custom downloads.
                                if (!applied) SelectedItem = e.RemovedItems[0];
                            }
                        )
                    );
                }
            }
        }
    }
}
