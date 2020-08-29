using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bluegrams.Application;
using Vividl.Model;
using Vividl.ViewModel;
using YoutubeDLSharp.Options;

namespace Vividl.Services
{
    /// <summary>
    /// Provides methods related to creating download entries.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IItemProvider<T> where T : IDownloadEntry
    {
        ItemViewModel<T> CreateItemViewModel(string url, MainViewModel<T> mainVm);
        Task<IEnumerable<ItemViewModel<T>>> FetchItemList(
            string[] itemUrls, ICollection<ItemViewModel<T>> itemVms,
            MainViewModel<T> mainVm, IDialogService dialogService,
            int? selectedFormat = null, OptionSet overrideOptions = null);
    }
}
