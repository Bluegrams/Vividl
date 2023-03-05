$ErrorActionPreference = 'Stop';
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"


$archive = Join-Path $toolsDir "vividl.zip"
Get-ChocolateyUnzip -FileFullPath $archive -Destination $toolsDir -PackageName $env:ChocolateyPackageName

$targetDir = Join-Path $toolsDir "Vividl"
$target =  Join-Path $targetDir "Vividl.exe"
Install-ChocolateyShortcut -shortcutFilePath "$([Environment]::GetFolderPath('CommonDesktopDirectory'))\Vividl.lnk" -targetPath $target -workingDirectory $targetDir
Install-ChocolateyShortcut -shortcutFilePath "$([Environment]::GetFolderPath('CommonStartMenu'))\Programs\Bluegrams\Vividl.lnk" -targetPath $target -workingDirectory $targetDir

New-Item -Path "$target.gui" -ItemType File -Force | Out-Null
