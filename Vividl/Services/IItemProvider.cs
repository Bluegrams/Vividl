using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bluegrams.Application;
using Vividl.Model;
using Vividl.ViewModel;

namespace Vividl.Services
{
    /// <summary>
    /// Provides methods related to creating download entries.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IItemProvider<T> where T : IDownloadEntry
    {
        ItemViewModel<T> CreateItemViewModel(string url, MainViewModel<T> mainVm);
        Task FetchItemList(string[] itemUrls, ICollection<ItemViewModel<T>> itemVms, MainViewModel<T> mainVm, IDialogService dialogService);
    }
}
