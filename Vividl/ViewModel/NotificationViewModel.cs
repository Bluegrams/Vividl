using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Enterwell.Clients.Wpf.Notifications.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Vividl.Services;

namespace Vividl.ViewModel
{
    public class NotificationViewModel : ViewModelBase
    {
        private readonly NotificationDialogService dialogService;

        public ObservableCollection<NotificationMessage> NotificationLog { get; }

        public bool HasNotifications => NotificationLog.Count > 0;

        public ICommand ClearAllCommand { get; }

        public NotificationViewModel(NotificationDialogService dialogService)
        {
            NotificationLog = new ObservableCollection<NotificationMessage>();
            this.dialogService = dialogService;
            this.dialogService.NotificationAdded += DialogService_NotificationAdded;
            this.dialogService.NotificationRemoved += DialogService_NotificationRemoved;
            // Init commands
            ClearAllCommand = new RelayCommand(() => ClearAll());
        }

        public void ClearAll()
        {
            NotificationLog.Clear();
            RaisePropertyChanged(null);
        }

        private void DialogService_NotificationAdded(object sender, NotificationEventArgs e)
        {
            NotificationLog.Add(e.Message);
            RaisePropertyChanged(null);
        }

        private void DialogService_NotificationRemoved(object sender, NotificationEventArgs e)
        {
            NotificationLog.Remove(e.Message);
            RaisePropertyChanged(null);
        }
    }
}
