# This script downloads FFmpeg and yt-dlp for Windows.

# Download ffmpeg
echo "Downloading ffmpeg..."
$ffmpeg_meta_url = "https://ffbinaries.com/api/v1/version/latest"
$data = Invoke-WebRequest $ffmpeg_meta_url | ConvertFrom-Json
echo "Found ffmpeg version: $($data.version)"
echo $data.version > (Join-Path $PSScriptRoot "ffmpeg-version.txt")
$ffmpeg_download_url = $data.bin.'windows-64'.ffmpeg
$ffmpeg_archive = Join-Path $PSScriptRoot "ffmpeg.zip"
Invoke-WebRequest -Uri $ffmpeg_download_url -OutFile $ffmpeg_archive
Expand-Archive -Path $ffmpeg_archive -DestinationPath $PSScriptRoot -Force
Remove-Item $ffmpeg_archive

# Download yt-dlp
echo "Downloading yt-dlp..."
$ytdl_url = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe"
$ytdl_exe = Join-Path $PSScriptRoot "yt-dlp.exe"
Invoke-WebRequest -Uri $ytdl_url -OutFile $ytdl_exe
$version = $(& $ytdl_exe --version)
echo "yt-dlp version: $version"
echo $version > (Join-Path $PSScriptRoot "youtube-dl-version.txt")

echo "Downloads finished."
