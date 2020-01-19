using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Vividl.Services;

namespace Vividl.View
{
    public partial class PathSelectionControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty SelectedPathProperty =
            DependencyProperty.Register("SelectedPath",
                typeof(string), typeof(PathSelectionControl),
                new PropertyMetadata("", onDependencyPropertyChanged)
            );
        public static readonly DependencyProperty FileServiceProperty =
            DependencyProperty.Register("FileService",
                typeof(IFileService), typeof(PathSelectionControl),
                new PropertyMetadata(null, onDependencyPropertyChanged)
            );
        public static readonly DependencyProperty IsSelectFolderProperty =
            DependencyProperty.Register("IsSelectFolder",
                typeof(bool), typeof(PathSelectionControl),
                new PropertyMetadata(false, onDependencyPropertyChanged)
            );

        public PathSelectionControl()
        {
            InitializeComponent();
        }

        public string SelectedPath
        {
            get => (string)GetValue(SelectedPathProperty);
            set => SetValue(SelectedPathProperty, value);
        }

        public IFileService FileService
        {
            get => (IFileService)GetValue(FileServiceProperty);
            set => SetValue(FileServiceProperty, value);
        }

        public bool IsSelectFolder
        {
            get => (bool)GetValue(IsSelectFolderProperty);
            set => SetValue(IsSelectFolderProperty, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private static void onDependencyPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is PathSelectionControl sender)
            {
                sender.onPropertyChanged(args.Property.Name);
            }
        }

        private void onPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // TODO description missing in service
            string newPath;
            if (IsSelectFolder)
                newPath = FileService?.SelectFolder(selected: SelectedPath);
            else newPath = FileService?.SelectOpenFile(selected: SelectedPath);
            if (!String.IsNullOrWhiteSpace(newPath))
            {
                SelectedPath = newPath;
            }
        }
    }
}
