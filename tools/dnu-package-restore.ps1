$toolsPath = split-path $MyInvocation.MyCommand.Definition
$solutionPath = [System.IO.Path]::GetFullPath($(join-path $toolsPath ".."))
$dnvm = join-path $toolsPath "dnvm.ps1"

& $dnvm install 1.0.0-beta4
& $dnvm use 1.0.0-beta4
& dnu restore $solutionPath
