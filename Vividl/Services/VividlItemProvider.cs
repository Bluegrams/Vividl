using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bluegrams.Application;
using Vividl.Model;
using Vividl.ViewModel;
using YoutubeDLSharp.Options;

namespace Vividl.Services
{
    public class VividlItemProvider : IItemProvider<MediaEntry>
    {
        public ItemViewModel<MediaEntry> CreateItemViewModel(string url, MainViewModel<MediaEntry> mainVm)
            => new VideoViewModel(url, mainVm);

        public async Task<IEnumerable<ItemViewModel<MediaEntry>>> FetchItemList(
            string[] itemUrls, ICollection<ItemViewModel<MediaEntry>> itemVms,
            MainViewModel<MediaEntry> mainVm, IDialogService dialogService, int? selectedFormat = null, OptionSet overrideOptions = null)
        {
            var tasks = new Dictionary<ItemViewModel<MediaEntry>, Task>();
            List<ItemViewModel<MediaEntry>> fetchedVideos = new List<ItemViewModel<MediaEntry>>();
            foreach (var url in itemUrls)
            {
                var videoVm = CreateItemViewModel(url, mainVm);
                itemVms.Add(videoVm);
                fetchedVideos.Add(videoVm);
                tasks.Add(videoVm, videoVm.Fetch(overrideOptions: overrideOptions).ContinueWith((t) => {
                    if (selectedFormat.HasValue)
                    {
                        videoVm.SelectedDownloadOption = selectedFormat.Value;
                    }
                }));
            }
            await Task.WhenAll(tasks.Values.ToArray());
            return fetchedVideos;
        }
    }
}
