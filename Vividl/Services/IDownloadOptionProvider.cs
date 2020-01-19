using System;
using System.Collections.Generic;
using Vividl.Model;

namespace Vividl.Services
{
    public interface IDownloadOptionProvider
    {
        List<IDownloadOption> CreateDownloadOptions();
    }
}
