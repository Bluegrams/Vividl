using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Vividl.Services
{
    public interface IFileService
    {
        string SelectOpenFile(string filter = null, string selected = null);

        string SelectSaveFile(string filter = null);

        string SelectFolder(string description = null, string selected = null);

        void ShowInExplorer(string path, bool isFolder = false);

        void ShowInExplorer(string[] paths);
    }

    class FileService : IFileService
    {
        public string SelectOpenFile(string filter = null, string selected = null)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = filter;
            ofd.FileName = selected;
            ofd.ShowDialog();
            return ofd.FileName;
        }

        public string SelectSaveFile(string filter = null)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = filter;
            sfd.ShowDialog();
            return sfd.FileName;
        }

        public string SelectFolder(string description = null, string selected = null)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = description;
            fbd.SelectedPath = selected;
            fbd.ShowDialog();
            return fbd.SelectedPath;
        }

        public void ShowInExplorer(string path, bool isFolder = false)
        {
            string command = isFolder ? "/open" : "/select";
            Process.Start("explorer.exe", $"{command},\"{path}\"");
        }

        public void ShowInExplorer(string[] paths)
        {
            var args = String.Join(" ", paths.Select(p => $"/select,{p}"));
            Process.Start("explorer.exe", args);
        }
    }
}
