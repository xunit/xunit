#Requires -Version 5.1

param(
    [ValidateSet('Build','BuildAll','CI','FormatSource','PackageRestore','Packages','Restore','Test',
                 '_AnalyzeSource', '_Packages','_Publish','_PushPackages','_SignPackages','_Test32','_Test64','_TestCore')]
    [string]$target = "BuildAll",
    [string]$configuration = "Release"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if ($null -eq $PSScriptRoot) {
    write-host "This build script requires PowerShell 3 or later." -ForegroundColor Red
    exit -1
}

Set-Location $PSScriptRoot

$packageOutputFolder = (join-path (Get-Location) "artifacts\packages")
$parallelFlags = "-parallel all"
$nonparallelFlags = "-parallel collections"
$testOutputFolder = (join-path (Get-Location) "artifacts\test")
$dotnetFormatCommand = "& dotnet dotnet-format --folder --exclude src/common/AssemblyResolution/Microsoft.DotNet.PlatformAbstractions --exclude src/common/AssemblyResolution/Microsoft.Extensions.DependencyModel --exclude src/xunit.assert/Asserts"

# Helper functions

function _build_step([string] $message) {
    Write-Host -ForegroundColor White $("==> " + $message + " <==")
    Write-Host ""
}

function _exec([string] $command, [string] $message = "") {
    if ($message -eq "") {
        $message = $command
    }
    Write-Host -ForegroundColor DarkGray ("EXEC: " + $message)
    Write-Host ""
    Invoke-Expression $command
    Write-Host ""

    if ($LASTEXITCODE -ne 0) {
        exit 1
    }
}

function _fatal([string] $message) {
    Write-Host -ForegroundColor Red ("Error: " + $message)
    exit -1
}

function _mkdir([string] $path) {
    if ((test-path $path) -eq $false) {
        New-Item -Type directory -Path $path | out-null
    }
}

function _msbuild([string] $project, [string] $configuration, [string] $target = "build", [string] $verbosity = "minimal", [string] $message = "", [string] $binlogFile = "") {
    $cmd = "msbuild " + $project + " /t:" + $target + " /p:Configuration=" + $configuration + " /v:" + $verbosity + " /m"
    if ($binlogFile -ne "") {
        $cmd = $cmd + " /bl:" + $binlogFile
    }
    _exec $cmd $message
}

function _require([string] $command, [string] $message) {
    if ($null -eq (get-command $command -ErrorAction SilentlyContinue)) {
        _fatal $message
    }
}

function _verify_version([string]$version, [string]$minVersion, [string]$appName) {
    $dashIndex = $version.IndexOf('-')
    if ($dashIndex -gt -1) {
        $version = $version.Substring(0, $dashIndex)
    }

    if ([version]$version -lt [version]$minVersion) {
        _fatal ("Unsupported " + $appName + " version '$version' (must be '$minVersion' or later).")
    }
}

function _verify_dotnetsdk_version([string]$minVersion) {
    _verify_version (& dotnet --version) $minVersion ".NET SDK"
}

function _verify_msbuild_version([string]$minVersion) {
    _verify_version (& msbuild /nologo /ver) $minVersion "MSBuild"
}
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

function __target_build() {
    __target_restore
    __target__analyzesource

    _build_step "Compiling binaries"
        _msbuild "xunit.sln" $configuration
        _msbuild "src\xunit.console\xunit.console.csproj" ($configuration + "_x86")
}

function __target_buildall() {
    if ($null -ne $env:CI) {
        $script:parallelFlags = "-parallel none -maxthreads 1"
        $script:nonparallelFlags = "-parallel none -maxthreads 1"
    }

    __target_test
    __target__publish
    __target__packages
}

function __target_ci() {
    __target_buildall
    __target__signpackages
    __target__pushpackages
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
        _msbuild "src\xunit.console\xunit.console.csproj /p:TargetFramework=net6.0"        $configuration "publish"
}

function __target__pushpackages() {
    _build_step "Pushing NuGet packages"
        if (($null -eq $env:PUSH_APIKEY) -or
            ($null -eq $env:PUSH_URI))
        {
            Write-Host -ForegroundColor Yellow "Skipping package push because of one or more missing environment variables: 'PUSH_APIKEY', 'PUSH_URI'"
            Write-Host ""
        } else {
            Get-ChildItem -Filter *.nupkg $packageOutputFolder | ForEach-Object {
                $cmd = '& dotnet nuget push --source ' + $env:PUSH_URI + ' --api-key ' + $env:PUSH_APIKEY + ' "' + $_.FullName + '"'
                $message = $cmd.Replace($env:PUSH_APIKEY, "[redacted]")
                _exec $cmd $message
            }
        }
}

function __target__signpackages() {
    _build_step "Signing NuGet packages"
        if (($null -eq $env:SIGN_APP_ID) -or
            ($null -eq $env:SIGN_APP_SECRET) -or
            ($null -eq $env:SIGN_CERT_NAME) -or
            ($null -eq $env:SIGN_TENANT) -or
            ($null -eq $env:SIGN_TIMESTAMP_URI) -or
            ($null -eq $env:SIGN_VAULT_URI))
        {
            Write-Host -ForegroundColor Yellow "Skipping package sign because of one or more missing environment variables: 'SIGN_APP_ID', 'SIGN_APP_SECRET', 'SIGN_CERT_NAME', 'SIGN_TENANT', 'SIGN_TIMESTAMP_URI', 'SIGN_VAULT_URI'"
            Write-Host ""
        } else {
            _exec "& dotnet tool restore"

            $cmd = `
                '& dotnet sign code azure-key-vault **/*.nupkg' + `
                ' --base-directory "' + $packageOutputFolder + '"' + `
                ' --description "xUnit.net"' + `
                ' --description-url https://github.com/xunit' + `
                ' --timestamp-url ' + $env:SIGN_TIMESTAMP_URI + `
                ' --azure-key-vault-url ' + $env:SIGN_VAULT_URI + `
                ' --azure-key-vault-client-id ' + $env:SIGN_APP_ID + `
                ' --azure-key-vault-client-secret "' + $env:SIGN_APP_SECRET + '"' + `
                ' --azure-key-vault-tenant-id ' + $env:SIGN_TENANT + `
                ' --azure-key-vault-certificate ' + $env:SIGN_CERT_NAME

            $msg = $cmd.Replace($env:SIGN_VAULT_URI, '[redacted]')
            $msg = $msg.Replace($env:SIGN_APP_ID, '[redacted]')
            $msg = $msg.Replace($env:SIGN_APP_SECRET, '[redacted]')
            $msg = $msg.Replace($env:SIGN_TENANT, '[redacted]')
            $msg = $msg.Replace($env:SIGN_CERT_NAME, '[redacted]')
            _exec $cmd $msg
        }
}

function __target__test32() {
    _build_step "Running tests: 32-bit .NET 4.x"
        $v2_assemblies = [System.String]::Join(" ", (Get-ChildItem -Recurse -Include test.xunit.*.dll | Where-Object { $_.FullName -match "bin\\" + $configuration + "\\net452" } | ForEach-Object { $_.FullName }))
        _xunit_x86 ("test\test.xunit1\bin\" + $configuration + "\net40\test.xunit1.dll -xml artifacts\test\v1-x86.xml -html artifacts\test\v1-x86.html "            + $nonparallelFlags)
        _xunit_x86 ($v2_assemblies                                                 + " -xml artifacts\test\v2-x86.xml -html artifacts\test\v2-x86.html -serialize " + $parallelFlags)
}

function __target__test64() {
    _build_step "Running tests: 64-bit .NET 4.x"
        $v2_assemblies = [System.String]::Join(" ", (Get-ChildItem -Recurse -Include test.xunit.*.dll | Where-Object { $_.FullName -match "bin\\" + $configuration + "\\net452" } | ForEach-Object { $_.FullName }))
        _xunit_x64 ("test\test.xunit1\bin\" + $configuration + "\net40\test.xunit1.dll -xml artifacts\test\v1-x64.xml -html artifacts\test\v1-x64.html "            + $nonparallelFlags)
        _xunit_x64 ($v2_assemblies                                                 + " -xml artifacts\test\v2-x64.xml -html artifacts\test\v2-x64.html -serialize " + $parallelFlags)
}

function __target__testcore() {
    _build_step "Running tests: .NET Core 2.0"
        $netcore_assemblies = [System.String]::Join(" ", (Get-ChildItem -Recurse -Include test.xunit.*.dll | Where-Object { $_.FullName -match "bin\\" + $configuration + "\\netcoreapp2.0" } | ForEach-Object { $_.FullName }))
        _xunit_netcore "netcoreapp2.0" ($netcore_assemblies                        + " -xml artifacts\test\v2-netcore.xml -html artifacts\test\v2-netcore.html -serialize " + $nonparallelFlags)

    _build_step "Running tests: .NET 6"
        $net6_assemblies = [System.String]::Join(" ", (Get-ChildItem -Recurse -Include test.xunit.*.dll | Where-Object { $_.FullName -match "bin\\" + $configuration + "\\net6.0" } | ForEach-Object { $_.FullName }))
        _xunit_netcore "net6.0"        ($net6_assemblies                           + " -xml artifacts\test\v2-net6.xml -html artifacts\test\v2-net6.html -serialize " + $nonparallelFlags)
}

# Dispatch

$targetFunction = (Get-ChildItem ("Function:__target_" + $target.ToLowerInvariant()) -ErrorAction SilentlyContinue)
if ($null -eq $targetFunction) {
    _fatal "Unknown target '$target'"
}

_build_step "Performing pre-build verifications"
    _require dotnet "Could not find 'dotnet'. Please ensure .NET SDK 8.0 or later is installed."
    _verify_dotnetsdk_version "8.0"
    _require msbuild.exe "Could not find 'msbuild'. Please ensure MSBUILD.EXE v17.0 is on the path."
    _verify_msbuild_version "17.0.0"

_mkdir $packageOutputFolder
_mkdir $testOutputFolder
& $targetFunction
