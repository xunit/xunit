#!/usr/bin/env pwsh
#Requires -Version 5.1

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($null -eq (Get-Command "git" -ErrorAction Ignore)) {
	throw "Could not find 'git'; please install the Git command line tooling"
}

& git submodule status | ForEach-Object {
	if ($_[0] -eq '-') {
		$pieces = $_.Split(' ')
		& git submodule update --init "$($pieces[1])"
		Write-Host ""
	}
}

if ($null -eq (Get-Command "dotnet" -ErrorAction Ignore)) {
	throw "Could not find 'dotnet'; please install the  .NET Core SDK"
}

$version = [Version]$([regex]::matches((&dotnet --version), '^(\d+\.)?(\d+\.)?(\*|\d+)').value)
if ($version.Major -lt 8) {
	throw ".NET SDK version ($version) is too low; please install version 8.0 or later"
}

if ($null -eq (Get-Command "msbuild.exe" -ErrorAction Ignore)) {
	throw "Could not find 'msbuild.exe'; please run this from a Visual Studio developer shell"
}

Push-Location (Split-Path $MyInvocation.MyCommand.Definition)

try {
	& dotnet run --project tools/builder --no-launch-profile -- $args
}
finally {
	Pop-Location
}
