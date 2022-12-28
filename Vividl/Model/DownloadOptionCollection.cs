using System;
using System.Collections.Generic;
using System.Linq;

namespace Vividl.Model
{
    public class DownloadOptionCollection : List<IDownloadOption>
    {
        public int CustomDownloadIndex
            => this.IndexOfFirstOrDefault(f => f is CustomDownload);

        public CustomDownload CustomDownload
        {
            get => (CustomDownload)this.FirstOrDefault(f => f is CustomDownload);
            set
            {
                this[CustomDownloadIndex] = value;
            }
        }

        public int IndexOfFirstOrDefault(Func<IDownloadOption, bool> predicate)
            => this.Select((v, i) => new { value = v, index = i + 1})
                   .Where(pair => predicate(pair.value))
                   .Select(pair => pair.index)
                   .FirstOrDefault() - 1;
    }
}
