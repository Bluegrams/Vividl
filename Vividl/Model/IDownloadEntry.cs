using System;
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

        Task<DownloadResult> DownloadVideo(IDownloadOption downloadOption);
        void CancelDownload();
        void OpenInBrowser();
        void OpenFile();
        void ShowInFolder(IFileService fileService);
    }
}
