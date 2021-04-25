using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vividl.Services;

namespace Vividl.Model
{
    public interface IDownloadEntry
    {
        string Url { get; }
        string Title { get; }
        int TotalItems { get; }
        bool FileAvailable { get; }

        DownloadOptionCollection DownloadOptions { get; }
        int SelectedDownloadOption { get; set; }

        /// <summary>
        /// Downloads the selected download option.
        /// </summary>
        /// <returns>A DownloadResult object representing the outcome of the download.</returns>
        Task<DownloadResult> Download();
        void CancelDownload();
        void OpenInBrowser();
        void OpenFile();
        void ShowInFolder(IFileService fileService);
    }
}
