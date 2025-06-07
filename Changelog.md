# Vividl Changelog

### v.0.9.0 (2025-06)
- **New:** Hungarian, Korean translation.
- **Updated:** Chinese, Italian and Japanese translation.
- **Updated:** Extend resolution enum.
- **Fixed:** Preserving settings & preferences when updating the app.
- **Fixed:** Don't add duplicates in automation mode.
- **Fixed:** Use relative binary paths in portable mode.
- **Fixed:** Ensure direct format download have audio.
- **Fixed:** Ensure download progress is shown from yt-dlp.

### v.0.8.0 (2024-01)
- **New:** Allow specifying preferred video resolution in settings & fetch window.
- **New:** Support for editing video download names before download.
- **New:** Add notification log window & notifications button in status bar.
- **New:** Cache links of unfinished downloads between app restarts by default.
- **New:** Add "best audio-only download" to default download formats.
- **New:** Chinese translation.
- **Updated:** Rewrite of download format selection to better target yt-dlp.
- **Updated:** Remove invalid formats from download customization window; order by format preference.
- **Updated:** Japanese translation
- **Fixed:** Overwrite/ re-download handling for "Best direct download".
- **Fixed:** App crashed when exporting video links with unavailable videos.
- **Fixed:** Scrolling of settings window for larger text sizes.
- **Fixed:** Settings not correctly saved when closed via "Exit" menu or changed in different tab.
- **Fixed:** Video info parsing errors causing app crashes during fetching.

### v.0.7.0 (2023-03)
- **New:** Add drag & drop support for importing URLs.
- **New:** Add GPU acceleration option for AMD (AMF) and Intel (QSV) GPUs.
- **New:** Japanese translation.
- **Updated:** Dutch, Spanish and Russian translation.
- **Fixed:** Various issues causing crashes with newer yt-dlp versions.
- **Fixed:** App crashes with "Best direct download" option.
- **Fixed:** Missing download progress bar for some downloads.
- **Fixed:** Issues with audio-only downloads in custom download window.
- **Fixed:** Remove unnecessary yt-dlp warnings.

#### v.0.7.1 (2023-04)
- **Fixed:** Crashes when importing videos from certain sites.
- **Fixed:** Crashes when attempting to play removed files.
- **Updated:** Japanese and Italian translation.

### v.0.6.0 (2022-01)
- **New:** Smart Automation mode: Auto-import & download URLs from clipboard
- **New:** Add option for CUDA-supported FFmpeg conversion.
- **New:** Add thumbnails to mp3 conversion downloads (with "Add metadata to files" setting).
- **New:** Minor UI tweaks (e.g. in main menu & tool bar)
- **New:** Allow setting custom downloader arguments.
- **Changed:** Switch from youtube-dl to yt-dlp as default download engine (faster download speed).
- **Fixed:** Crash with invalid URLs in fetch window.

### v.0.5.0 (2021-06)
- **New:** Customize download in "Configure Download" window
- **New:** Show download size and resolution in "Configure Download" window
- **New:** UI improvements: show video thumbnails; add settings to toolbar
- **New:** Add mode to re-download videos with new name
- **New:** Simple automatic update checking for youtube-dl
- **New:** Add command to reload all videos
- **Changed:** Put youtube-dl into `AppData` folder by default

#### v.0.5.1 (2021-06)
- **Fixed:** App crashes when adding videos via "Add New Videos" window

### v.0.4.0 (2020-11)
- **New:** Italian, Hebrew, Polish, Welsh, Portuguese, Arabic, French, Dutch and Swedish translation
- **Updated:** Russian and Spanish translation

### v.0.3.0 (2020-10)
- **New:** Extended window for adding video links.
- **New:** Basic authentication support.
- **New:** Added window showing youtube-dl download output.
- **New:** Download progress information in main UI.
- **New:** Minor improvements in UI and settings (e.g. proxy).
- **New:** Spanish translation

### v.0.2.2 (2020-05)
- **New:** German and Russian translation.
- **Fixed:** Bug preventing app start for some locales.
- **Fixed:** FFmpeg warning messages during stream download.

### v.0.2.1 (2020-03)
- **New:** Option to disable download checking.
- **Fixed:** Download failures of some download formats.

### v.0.2.0 (2020-03)
- **New:** Use download archive.
- **New:** Remove all unavailable/ finished downloads.
- **New:** Minor UI improvements.
- **Fixed:** App crashes for videos from some websites (e.g. Vimeo).

### v.0.1.1 (2020-01)
- **Fixed:** App crashes for failed fetches.
- **Fixed:** Saving of settings in portable version.
- **Fixed:** Tooltip labeling.

### v.0.1.0 (2020-01)
- First released version
