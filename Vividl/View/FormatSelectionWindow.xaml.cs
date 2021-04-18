using System;
using System.Windows.Controls;
using AdonisUI.Controls;
using Vividl.ViewModel;

namespace Vividl.View
{
    public partial class FormatSelectionWindow : AdonisWindow
    {
        public FormatSelectionWindow(FormatSelectionViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                ((ListBox)sender).ScrollIntoView(e.AddedItems[0]);
                ((ListBox)sender).UpdateLayout();
            }
        }
    }
}
