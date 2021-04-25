using System;
using System.Collections.Generic;
using System.Linq;

namespace Vividl.Model
{
    public class DownloadOptionCollection : List<IDownloadOption>
    {
        public int CustomDownloadIndex
            => this.IndexOf(this.FirstOrDefault(f => f is CustomDownload));

        public CustomDownload CustomDownload
        {
            get => (CustomDownload)this.FirstOrDefault(f => f is CustomDownload);
            set
            {
                this[CustomDownloadIndex] = value;
            }
        }
    }
}
