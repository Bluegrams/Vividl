# This script downloads FFmpeg and youtube-dl for Windows.

# Download ffmpeg
echo "Downloading ffmpeg..."
$ffmpeg_meta_url = "https://ffbinaries.com/api/v1/version/latest"
$data = Invoke-WebRequest $ffmpeg_meta_url | ConvertFrom-Json
echo "Found ffmpeg version: $($data.version)"
$ffmpeg_download_url = $data.bin.'windows-64'.ffmpeg
$ffmpeg_archive = Join-Path $PSScriptRoot "ffmpeg.zip"
Invoke-WebRequest -Uri $ffmpeg_download_url -OutFile $ffmpeg_archive
Expand-Archive -Path $ffmpeg_archive -DestinationPath $PSScriptRoot -Force
Remove-Item $ffmpeg_archive

# Download youtube-dl
echo "Downloading youtube-dl..."
$ytdl_url = "https://yt-dl.org/latest/youtube-dl.exe"
$ytdl_exe = Join-Path $PSScriptRoot "youtube-dl.exe"
Invoke-WebRequest -Uri $ytdl_url -OutFile $ytdl_exe
$version = $(& $ytdl_exe --version)
echo "youtube-dl version: $version"
echo $version > (Join-Path $PSScriptRoot "youtube-dl-version.txt")

echo "Downloads finished."
