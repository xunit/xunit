$toolsPath = split-path $MyInvocation.MyCommand.Definition
$dnvm = join-path $toolsPath "dnvm.ps1"
$solutionPath = [System.IO.Path]::GetFullPath($(join-path $toolsPath ".."))
$globalJson = join-path $solutionPath "global.json"
$dnxVersion = (ConvertFrom-JSON ([System.IO.File]::ReadAllText($globalJson))).sdk.version

& $dnvm install $dnxVersion
& $dnvm use $dnxVersion
& dnu restore $solutionPath
