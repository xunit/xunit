#!/usr/bin/env pwsh
#Requires -Version 5.1

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($null -eq (Get-Command "dotnet" -ErrorAction Ignore)) {
    throw "Could not find 'dotnet'; please install the  .NET Core SDK"
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
