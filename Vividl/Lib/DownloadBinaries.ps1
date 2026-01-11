# This script downloads FFmpeg and yt-dlp for Windows.

# Download ffmpeg & ffprobe
echo "Downloading ffmpeg..."
$ffmpeg_meta_url = "https://www.gyan.dev/ffmpeg/builds/release-version"
$data = Invoke-WebRequest $ffmpeg_meta_url
echo "Found ffmpeg version: $($data)"
echo "$data" > (Join-Path $PSScriptRoot "ffmpeg-version.txt")
$ffmpeg_download_url = "https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-essentials.zip"
$ffmpeg_archive = Join-Path $PSScriptRoot "ffmpeg.zip"
Invoke-WebRequest -Uri $ffmpeg_download_url -OutFile $ffmpeg_archive
Expand-Archive -Path $ffmpeg_archive -DestinationPath $PSScriptRoot -Force
Move-Item (Join-Path $PSScriptRoot "ffmpeg-*-essentials_build\bin\*") $PSScriptRoot -Force
Remove-Item (Join-Path $PSScriptRoot "ffmpeg-*-essentials_build") -Recurse -Force
Remove-Item $ffmpeg_archive

# Download yt-dlp
echo "Downloading yt-dlp..."
$ytdl_url = "https://github.com/yt-dlp/yt-dlp/releases/latest/download/yt-dlp.exe"
$ytdl_exe = Join-Path $PSScriptRoot "yt-dlp.exe"
Invoke-WebRequest -Uri $ytdl_url -OutFile $ytdl_exe
$version = $(& $ytdl_exe --version)
echo "yt-dlp version: $version"
echo $version > (Join-Path $PSScriptRoot "youtube-dl-version.txt")

# Download quickjs
echo "Downloading QuickJS..."
$data = Invoke-WebRequest "https://bellard.org/quickjs/binary_releases/LATEST.json" | ConvertFrom-Json
Write-Output "Latest QuickJS version is: $($data.version)"
$target = "win-x86_64"
$downloadUrl = "https://bellard.org/quickjs/binary_releases/quickjs-${target}-$($data.version).zip"
$downloadZip = Join-Path $PSScriptRoot "quickjs.zip"
Invoke-WebRequest -Uri $downloadUrl -OutFile $downloadZip
Expand-Archive -Path $downloadZip -DestinationPath $PSScriptRoot -Force
Remove-Item $downloadZip

echo "Downloads finished."
