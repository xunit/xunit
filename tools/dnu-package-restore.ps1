$toolsPath = split-path $MyInvocation.MyCommand.Definition
$dnvm = join-path $toolsPath "dnvm.ps1"
$solutionPath = [System.IO.Path]::GetFullPath($(join-path $toolsPath ".."))
$globalJson = join-path $solutionPath "global.json"
$version = (ConvertFrom-JSON ([System.IO.File]::ReadAllText($globalJson))).sdk.version

& $dnvm install $version
& $dnvm use $version
& dnu restore $solutionPath
