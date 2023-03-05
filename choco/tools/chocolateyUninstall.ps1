$ErrorActionPreference = 'Stop';

Remove-Item "$([Environment]::GetFolderPath('CommonDesktopDirectory'))\Vividl.lnk"
Remove-Item "$([Environment]::GetFolderPath('CommonStartMenu'))\Programs\Bluegrams\Vividl.lnk"
