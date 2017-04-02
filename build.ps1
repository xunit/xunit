param(
    [string]$target = "test",
    [string]$configuration = "Release",
    [string]$buildAssemblyVersion = "",
    [string]$buildSemanticVersion = ""
)

if ($PSScriptRoot -eq $null) {
    write-host "This build script requires PowerShell 3 or later." -ForegroundColor Red
    exit -1
}

$buildModuleFile = join-path $PSScriptRoot "tools\build\xunit-build-module.psm1"

if ((test-path $buildModuleFile) -eq $false) {
    write-host "Could not find build module. Did you forget to 'git submodule update --init'?" -ForegroundColor Red
    exit -1
}

Set-StrictMode -Version 2
Import-Module $buildModuleFile -Scope Local -Force -ArgumentList "3.5.0"
Set-Location $PSScriptRoot

$packageOutputFolder = (join-path (Get-Location) "artifacts\packages")
$parallelFlags = "-parallel all -maxthreads 16"
$testOutputFolder = (join-path (Get-Location) "artifacts\test")

# Helper functions

function _xunit_x64([string]$command) {
    _exec ("src\xunit.console\bin\" + $configuration + "\net452\win7-x86\xunit.console.exe " + $command)
}

function _xunit_x86([string]$command) {
    _exec ("src\xunit.console\bin\" + $configuration + "_x86\net452\win7-x86\xunit.console.x86.exe " + $command)
}

# Top-level targets

function __target_appveyor() {
    __target_ci
    __target__pushmyget
}

function __target_build() {
    __target_packagerestore

    _build_step "Compiling binaries"
        _msbuild "xunit.vs2017.sln" $configuration
        _msbuild "src\xunit.console\xunit.console.csproj" ($configuration + "_x86")
        _dotnet ("publish src\xunit.console --framework netcoreapp1.0 --configuration " + $configuration + " /nologo /verbosity:minimal")
        _dotnet ("publish src\dotnet-xunit --framework netcoreapp1.0 --configuration " + $configuration + " /nologo /verbosity:minimal")
}

function __target_ci() {
    $script:parallelFlags = "-parallel none -maxthreads 1"

    __target__setversion
    __target_test
    __target__packages
}

function __target_packagerestore() {
    _build_step "Restoring NuGet packages"
        _dotnet "restore xunit.vs2017.sln"
}

function __target_packages() {
    __target_build
    __target__packages
}

function __target_register() {
    __target_packages
    __target__register
}

function __target_test() {
    __target_build
    __target__test32
    __target__test64
}

# Dependent targets

function __target__packages() {
    _build_step "Creating NuGet packages"
        Get-ChildItem -Recurse -Filter *.nuspec | _nuget_pack -outputFolder $packageOutputFolder -configuration $configuration
}

function __target__pushmyget() {
    _build_step "Pushing packages to MyGet"
        if ($env:MyGetApiKey -eq $null) {
            Write-Host -ForegroundColor Yellow "Skipping MyGet push because environment variable 'MyGetApiKey' is not set."
            Write-Host ""
        } else {
            Get-ChildItem -Filter *.nupkg $packageOutputFolder | _nuget_push -source https://www.myget.org/F/xunit/api/v2/package -apiKey $env:MyGetApiKey
        }
}

function __target__register() {
    _download_nuget

    _build_step "Registering dotnet-test to NuGet package cache"
        $nugetCachePath = Join-Path (Join-Path $env:HOME ".nuget") "packages"
        $packageInstallFolder = Join-Path (Join-Path $nugetCachePath "dotnet-xunit") "99.99.99-dev"
        if (Test-Path $packageInstallFolder) {
            Remove-Item $packageInstallFolder -Recurse -Force -ErrorAction SilentlyContinue
        }
        _exec ('& "' + $nugetExe + '" add artifacts\packages\dotnet-xunit.99.99.99-dev.nupkg -NonInteractive -Expand -Source "' + $nugetCachePath + '"')
}

function __target__setversion() {
    if ($buildAssemblyVersion -ne "") {
        _build_step ("Setting assembly version: '" + $buildAssemblyVersion + "'")
            Get-ChildItem -Recurse -Filter GlobalAssemblyInfo.cs | _replace -match '\("99\.99\.99\.0"\)' -replacement ('("' + $buildAssemblyVersion + '")')
    }

    if ($buildSemanticVersion -ne "") {
        _build_step ("Setting semantic version: '" + $buildSemanticVersion + "'")
            Get-ChildItem -Recurse -Filter GlobalAssemblyInfo.cs | _replace -match '\("99\.99\.99-dev"\)' -replacement ('("' + $buildSemanticVersion + '")')
            Get-ChildItem -Recurse -Filter *.nuspec | _replace -match '99\.99\.99-dev' -replacement $buildSemanticVersion
    }
}

function __target__test32() {
    _build_step "Running 32-bit unit tests"
        $v2_assemblies = Get-ChildItem -Recurse -Include test.xunit.*.dll | Where-Object { $_.FullName -match "bin\\Release\\net452" } | ForEach-Object { $_.FullName }
        _xunit_x86 ("test\test.xunit1\bin\" + $configuration + "\net40\test.xunit1.dll " + $parallelFlags + " -serialize -xml artifacts\test\v1-x86.xml -html artifacts\test\v1-x86.html")
        _xunit_x86 ($v2_assemblies                                                       + $parallelFlags + " -serialize -xml artifacts\test\v2-x86.xml -html artifacts\test\v2-x86.html")
}

function __target__test64() {
    _build_step "Running 64-bit unit tests"
        $v2_assemblies = Get-ChildItem -Recurse -Include test.xunit.*.dll | Where-Object { $_.FullName -match "bin\\Release\\net452" } | ForEach-Object { $_.FullName }
        _xunit_x64 ("test\test.xunit1\bin\" + $configuration + "\net40\test.xunit1.dll " + $parallelFlags + " -serialize -xml artifacts\test\v1-x64.xml -html artifacts\test\v1-x64.html")
        _xunit_x64 ($v2_assemblies                                                       + $parallelFlags + " -serialize -xml artifacts\test\v2-x64.xml -html artifacts\test\v2-x64.html")
}

# Dispatch

$targetFunction = (Get-ChildItem ("Function:__target_" + $target.ToLowerInvariant()) -ErrorAction SilentlyContinue)
if ($targetFunction -eq $null) {
    _fatal "Unknown target '$target'"
}

_build_step "Performing pre-build verifications"
    _require dotnet "Could not find 'dotnet'. Please ensure .NET CLI Tooling is installed."
    _require msbuild "Could not find 'msbuild'. Please ensure MSBUILD.EXE v15 is on the path."
    _verify_msbuild15

_mkdir $packageOutputFolder
_mkdir $testOutputFolder
& $targetFunction
