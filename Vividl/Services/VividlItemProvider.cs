using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bluegrams.Application;
using Vividl.Model;
using Vividl.Properties;
using Vividl.ViewModel;

namespace Vividl.Services
{
    public class VividlItemProvider : IItemProvider<MediaEntry>
    {
        public ItemViewModel<MediaEntry> CreateItemViewModel(string url, MainViewModel<MediaEntry> mainVm)
            => new VideoViewModel(url, mainVm);

        public async Task FetchItemList(
            string[] itemUrls, ICollection<ItemViewModel<MediaEntry>> itemVms,
            MainViewModel<MediaEntry> mainVm, IDialogService dialogService)
        {
            var tasks = new Dictionary<ItemViewModel<MediaEntry>, Task>();
            foreach (var url in itemUrls)
            {
                var videoVm = CreateItemViewModel(url, mainVm);
                itemVms.Add(videoVm);
                tasks.Add(videoVm, videoVm.Fetch());
            }
            await Task.WhenAll(tasks.Values.ToArray());
        }
    }
}
