$ErrorActionPreference = 'Stop';

Remove-Item "$HOME\Desktop\Vividl.lnk"
Remove-Item "$([Environment]::GetFolderPath('CommonStartMenu'))\Programs\Bluegrams\Vividl.lnk"
