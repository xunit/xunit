#!/usr/bin/env pwsh
#Requires -Version 5.1

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function GuardBin {
    param (
        [string]$binary,
        [string]$message
    )

    if ($null -eq (Get-Command $binary -ErrorAction Ignore)) {
        throw "Could not find '$binary'; $message"
    }
}

GuardBin git "please install the Git CLI from https://git-scm.com/"
GuardBin dotnet "please install the .NET SDK from https://dot.net/"

if ((get-content variable:IsLinux -ErrorAction Ignore) -or (get-content variable:IsMacOS -ErrorAction Ignore)) {
    GuardBin mono "please install Mono from https://www.mono-project.com/"
} else {
    GuardBin msbuild.exe "please run this from a Visual Studio developer shell"
}

$version = [Version]$([regex]::matches((&dotnet --version), '^(\d+\.)?(\d+\.)?(\*|\d+)').value)
if ($version.Major -lt 8) {
    throw ".NET SDK version ($version) is too low; please install version 8.0 or later from https://dot.net/"
}

& git submodule status | ForEach-Object {
    if ($_[0] -eq '-') {
        $pieces = $_.Split(' ')
        & git submodule update --init "$($pieces[1])"
        Write-Host ""
    }
}

Push-Location (Split-Path $MyInvocation.MyCommand.Definition)

try {
    & dotnet run --project tools/builder --no-launch-profile -- $args
}
finally {
    Pop-Location
}
