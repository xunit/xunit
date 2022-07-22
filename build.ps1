#Requires -Version 5.1

param(
    [ValidateSet('GitHubActions','Build','CI','FormatSource','PackageRestore','Packages','Restore','Test',
                 '_AnalyzeSource', '_Packages','_Publish','_PushMyGet','_SignPackages','_Test32','_Test64','_TestCore')]
    [string]$target = "Test",
    [string]$configuration = "Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($null -eq $PSScriptRoot) {
    write-host "This build script requires PowerShell 3 or later." -ForegroundColor Red
    exit -1
}

$buildModuleFile = join-path $PSScriptRoot "tools\build\xunit-build-module.psm1"

if ((test-path $buildModuleFile) -eq $false) {
    write-host "Could not find build module. Did you forget to 'git submodule update --init'?" -ForegroundColor Red
    exit -1
}

Set-StrictMode -Version 2
Import-Module $buildModuleFile -Scope Local -Force -ArgumentList "4.5.1"
Set-Location $PSScriptRoot

$packageOutputFolder = (join-path (Get-Location) "artifacts\packages")
$parallelFlags = "-parallel all -maxthreads 16"
$nonparallelFlags = "-parallel collections -maxthreads 16"
$testOutputFolder = (join-path (Get-Location) "artifacts\test")
$signClientAppSettings = (join-path (Get-Location) "tools\SignClient\appsettings.json")
$dotnetFormatCommand = "& dotnet dotnet-format --folder --exclude src/common/AssemblyResolution/Microsoft.DotNet.PlatformAbstractions --exclude src/common/AssemblyResolution/Microsoft.Extensions.DependencyModel --exclude src/xunit.assert/Asserts"

# Helper functions

function _xunit_x64([string]$command) {
    _exec ("src\xunit.console\bin\" + $configuration + "\net452\xunit.console.exe " + $command)
}

function _xunit_x86([string]$command) {
    _exec ("src\xunit.console\bin\" + $configuration + "_x86\net452\xunit.console.x86.exe " + $command)
}

function _xunit_netcore([string]$targetFramework, [string]$command) {
    _exec ("dotnet src\xunit.console\bin\" + $configuration + "\" + $targetFramework + "\xunit.console.dll " + $command)
}

# Top-level targets

function __target_githubactions() {
    __target_ci
    __target__signpackages
    __target__pushmyget
}

function __target_build() {
    __target_restore
    __target__analyzesource

    _build_step "Compiling binaries"
        _msbuild "xunit.sln" $configuration
        _msbuild "src\xunit.console\xunit.console.csproj" ($configuration + "_x86")
}

function __target_ci() {
    $script:parallelFlags = "-parallel none -maxthreads 1"
    $script:nonparallelFlags = "-parallel none -maxthreads 1"

    __target_test
    __target__publish
    __target__packages
}

function __target_formatsource() {
    _build_step "Formatting source"
        _exec "& dotnet tool restore"
        _exec $dotnetFormatCommand
}

function __target_packagerestore() {
    __target_restore
}

function __target_packages() {
    __target_build
    __target__publish
    __target__packages
}

function __target_restore() {
    _build_step "Restoring NuGet packages"
       _msbuild "xunit.sln" $configuration "restore"
}

function __target_test() {
    __target_build
    __target__test32
    __target__test64
    __target__testcore
}

# Dependent targets

function __target__analyzesource() {
    _build_step "Analyzing source (if this fails, run './build FormatSource' to fix)"
        _exec "& dotnet tool restore"
        _exec ($dotnetFormatCommand + " --check")
}

function __target__packages() {
    _build_step "Creating NuGet packages"
        Get-ChildItem -Path $packageOutputFolder -Recurse -Filter *.nupkg | Remove-Item
        Get-ChildItem -Recurse -Filter *.nuspec | ForEach-Object {
            _exec ('& dotnet pack --nologo --no-build --configuration ' + $configuration + ' --verbosity minimal --output "' + $packageOutputFolder + '" src/xunit.core -p:NuspecFile="' + $_.FullName + '"')
        }
}

function __target__publish() {
    _build_step "Publishing projects for packaging"
        _msbuild "src\xunit.console\xunit.console.csproj /p:TargetFramework=netcoreapp1.0" $configuration "publish"
        _msbuild "src\xunit.console\xunit.console.csproj /p:TargetFramework=netcoreapp2.0" $configuration "publish"
}

function __target__pushmyget() {
    _build_step "Pushing packages to MyGet"
        if ($null -eq $env:PublishToken) {
            Write-Host -ForegroundColor Yellow "Skipping MyGet push because environment variable 'PublishToken' is not set."
            Write-Host ""
        } else {
            Get-ChildItem -Filter *.nupkg $packageOutputFolder | ForEach-Object {
                $cmd = '& dotnet nuget push --source https://www.myget.org/F/xunit/api/v2/package --api-key ' + $env:PublishToken + ' "' + $_.FullName + '"'
                $message = $cmd.Replace($env:PublishToken, "[redacted]")
                _exec $cmd $message
            }
        }
}

function __target__signpackages() {
    if ($null -ne $env:SignClientSecret) {
        _build_step "Signing NuGet packages"
            _exec "& dotnet tool restore"

            $cmd = '& dotnet signclient sign --config "' + $signClientAppSettings + '" --user "' + $env:SignClientUser + '" --secret "' + $env:SignClientSecret + '" --name "xUnit.net" --description "xUnit.net" -u "https://github.com/xunit/xunit" --baseDirectory "' + $packageOutputFolder + '" --input **/*.nupkg'
            $msg = $cmd.Replace($env:SignClientSecret, '[Redacted]')
            $msg = $msg.Replace($env:SignClientUser, '[Redacted]')
            _exec $cmd $msg
    }
}

function __target__test32() {
    _build_step "Running tests: 32-bit .NET 4.x"
        $v2_assemblies = [System.String]::Join(" ", (Get-ChildItem -Recurse -Include test.xunit.*.dll | Where-Object { $_.FullName -match "bin\\" + $configuration + "\\net452" } | ForEach-Object { $_.FullName }))
        _xunit_x86 ("test\test.xunit1\bin\" + $configuration + "\net40\test.xunit1.dll -xml artifacts\test\v1-x86.xml -html artifacts\test\v1-x86.html "                               + $nonparallelFlags)
        _xunit_x86 ($v2_assemblies                                                 + " -xml artifacts\test\v2-x86.xml -html artifacts\test\v2-x86.html -appdomains denied -serialize " + $parallelFlags)
}

function __target__test64() {
    _build_step "Running tests: 64-bit .NET 4.x"
        $v2_assemblies = [System.String]::Join(" ", (Get-ChildItem -Recurse -Include test.xunit.*.dll | Where-Object { $_.FullName -match "bin\\" + $configuration + "\\net452" } | ForEach-Object { $_.FullName }))
        _xunit_x64 ("test\test.xunit1\bin\" + $configuration + "\net40\test.xunit1.dll -xml artifacts\test\v1-x64.xml -html artifacts\test\v1-x64.html "                               + $nonparallelFlags)
        _xunit_x64 ($v2_assemblies                                                 + " -xml artifacts\test\v2-x64.xml -html artifacts\test\v2-x64.html -appdomains denied -serialize " + $parallelFlags)
}

function __target__testcore() {
    _build_step "Running tests: .NET Core 2.0"
        $netcore_assemblies = [System.String]::Join(" ", (Get-ChildItem -Recurse -Include test.xunit.*.dll | Where-Object { $_.FullName -match "bin\\" + $configuration + "\\netcoreapp2.0" } | ForEach-Object { $_.FullName }))
        _xunit_netcore "netcoreapp2.0" ($netcore_assemblies                        + " -xml artifacts\test\v2-netcore.xml -html artifacts\test\v2-netcore.html -serialize "            + $nonparallelFlags)
}

# Dispatch

$targetFunction = (Get-ChildItem ("Function:__target_" + $target.ToLowerInvariant()) -ErrorAction SilentlyContinue)
if ($null -eq $targetFunction) {
    _fatal "Unknown target '$target'"
}

_build_step "Performing pre-build verifications"
    _require dotnet "Could not find 'dotnet'. Please ensure .NET SDK is installed."
    _require msbuild.exe "Could not find 'msbuild'. Please ensure MSBUILD.EXE v17.0 is on the path."
    _verify_msbuild_version "17.0.0"

_mkdir $packageOutputFolder
_mkdir $testOutputFolder
& $targetFunction
