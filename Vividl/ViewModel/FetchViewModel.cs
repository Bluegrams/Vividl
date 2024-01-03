using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using Vividl.Model;
using Vividl.Properties;
using YoutubeDLSharp.Options;

namespace Vividl.ViewModel
{
    public class FetchViewModel : ViewModelBase
    {
        private string[] videoUrls;
        private int selectedDownloadOption;
        private Resolution preferredResolution;
        private bool immediateDownload;

        public OptionSet OverrideOptions { get; }

        public string[] VideoUrls
        {
            get => videoUrls;
            set
            {
                videoUrls = value;
                RaisePropertyChanged();
            }
        }

        public bool ImmediateDownload
        {
            get => immediateDownload;
            set
            {
                immediateDownload = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<IDownloadOption> DownloadOptions
            => SimpleIoc.Default.GetInstance<SettingsViewModel>().DefaultFormats;

        public int SelectedDownloadOption
        {
            get => selectedDownloadOption;
            set
            {
                selectedDownloadOption = value;
                RaisePropertyChanged();
            }
        }

        public Resolution PreferredResolution
        {
            get => preferredResolution;
            set
            {
                preferredResolution = value;
                OverrideOptions.FormatSort = value.ToFormatSort();
                RaisePropertyChanged();
            }
        }

        public ICommand SettingsCommand { get; }

        public FetchViewModel()
        {
            OverrideOptions = new OptionSet();
            SelectedDownloadOption = Settings.Default.DefaultFormat;
            PreferredResolution = Settings.Default.DefaultResolution;
            SettingsCommand = new RelayCommand(
                () => Messenger.Default.Send(new ShowWindowMessage(WindowType.SettingsWindow, 0))
            );
        }
    }
}
