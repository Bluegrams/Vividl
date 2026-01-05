using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Bluegrams.Application;
using Vividl.Properties;
using YoutubeDLSharp;

namespace Vividl.Services.Update
{
    public class FFmpegUpdateService : ILibUpdateService, INotifyPropertyChanged
    {
        public const string FFMPEG_API_URL = "https://www.gyan.dev/ffmpeg/builds/release-version";
        public const string FFMPEG_DOWNLOAD_URL = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip";

        private IDialogService dialogService;
        private string version;
        private bool isUpdating;

        public FFmpegUpdateService(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            setVersion();
        }

        public string FFmpegPath => Settings.Default.FfmpegPath;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Version
        {
            get => version;
            set
            {
                version = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Version)));
            }
        }

        public bool IsUpdating
        {
            get => isUpdating;
            set
            {
                isUpdating = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUpdating)));
            }
        }

        private void setVersion()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Utils.GetFullPath(FFmpegPath),
                    Arguments = "-version",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (Process process = Process.Start(startInfo))
                {
                    string output = process.StandardOutput.ReadLine();
                    // Example output: "ffmpeg version 4.4.1 Copyright (c) ..."
                    string[] parts = output.Split(' ');
                    if (parts.Length >= 3)
                    {
                        Version = parts[2].Split('-')[0];
                    }
                    else
                    {
                        Version = "Unknown";
                    }
                }
            }
            catch (Exception)
            {
                Version = "Unknown";
            }
        }

        public async Task<bool> CheckForUpdates()
        {
            IsUpdating = true;
            setVersion();
            using (WebClient client = new WebClient())
            {
                try
                {
                    string latestVersion = await client.DownloadStringTaskAsync(FFMPEG_API_URL);
                    Debug.WriteLine("[FFmpeg update check] Found version: " + latestVersion);
                    if (new Version(latestVersion) > new Version(this.Version))
                    {
                        return dialogService.ShowConfirmation(
                            String.Format(Resources.YtdlUpdateService_NewUpdateMessage, latestVersion, this.Version, "FFmpeg"),
                            "Vividl - " + Resources.Info
                        );
                    }
                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    IsUpdating = false;
                }
            }
        }

        public async Task<string> Update()
        {
            IsUpdating = true;
            var downloadDir = Path.GetDirectoryName(FFmpegPath);
            using (WebClient client = new WebClient())
            {
                var dataBytes = await client.DownloadDataTaskAsync(FFMPEG_DOWNLOAD_URL);
                using (var stream = new MemoryStream(dataBytes))
                {
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        if (archive.Entries.Count > 0)
                        {
                            var binFiles = archive.Entries.Where(e => e.FullName.EndsWith("ffmpeg.exe") || e.FullName.EndsWith("ffprobe.exe")).ToList();
                            Debug.WriteLine("Extracting downloaded files: " + String.Join(",", binFiles.Select(f => f.Name).ToArray()));
                            foreach (var binFile in binFiles)
                            {
                                string outPath = Path.Combine(downloadDir, binFile.Name);
                                binFile.ExtractToFile(outPath, true);
                            }
                        }
                    }
                }
            }
            setVersion();
            IsUpdating = false;
            return $"FFmpeg and FFprobe now at version {Version}";
        }
    }
}
