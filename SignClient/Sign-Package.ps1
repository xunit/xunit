$currentDirectory = split-path $MyInvocation.MyCommand.Definition

# See if we have the ClientSecret available
if([string]::IsNullOrEmpty($env:SignClientSecret)){
	Write-Host "Client Secret not found, not signing packages"
	return;
}

# Setup Variables we need to pass into the sign client tool

$appSettings = "$currentDirectory\appsettings.json"

$appPath = "$currentDirectory\..\packages\SignClient\tools\netcoreapp2.0\SignClient.dll"

$nupgks = ls $currentDirectory\..\artifacts\packages\*.zip | Select -ExpandProperty FullName

foreach ($nupkg in $nupgks){
	Write-Host "Submitting $nupkg for signing"

	dotnet $appPath 'sign' -c $appSettings -i $nupkg -s $env:SignClientSecret -n 'xUnit.net' -d 'xUnit.net' -u 'https://github.com/xunit/xunit' 

	Write-Host "Finished signing $nupkg"
}

Write-Host "Sign-package complete"