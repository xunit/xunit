param(
    [string]$target = "test",
    [string]$configuration = "Release",
    [string]$buildAssemblyVersion = "",
    [string]$buildSemanticVersion = ""
)

$parallelFlags = "-parallel all"
$nugetVersion = "3.5.0"

# Helper functions

function _build_step([string] $message) {
    Write-Host -ForegroundColor White $("==> " + $message + " <==")
    Write-Host ""
}

function _dotnet([string] $command, [string] $message = "") {
    _exec ("dotnet " + $command) $message
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
    exit 1
}

function _msbuild([string] $project, [string] $config, [string] $target = "build", [string] $verbosity = "minimal", [string] $message = "") {
    _exec ("msbuild " + $project + " /t:" + $target + " /p:Configuration=" + $config + " /v:" + $verbosity + " /m /nologo") $message
}

function _replace([string] $file, [regex]$match, [string]$replacement) {
    $content = Get-Content -raw $file
    $content = $match.Replace($content, $replacement)
    Set-Content $file $content -Encoding UTF8 -NoNewline
}

function _require([string] $command, [string] $message) {
    if ((get-command $command -ErrorAction SilentlyContinue) -eq $null) {
        _fatal $message
    }
}

function _verify_msbuild15() {
    $version = & msbuild /nologo /ver

    if (-not $version.StartsWith("15.")) {
        _fatal "Unexpected MSBUILD version $version. Please ensure MSBUILD.EXE v15 is on the path."
    }
}

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

function __target_test() {
    __target_build
    __target__test32
    __target__test64
}

# Dependent targets

function __target__downloadnuget() {
    $cliVersionPath = join-path $home (".nuget\cli\" + $nugetVersion)
    New-Item -Type Directory -Path $cliVersionPath -ErrorAction SilentlyContinue | out-null

    $script:nugetExe = join-path $cliVersionPath "nuget.exe"
    if ((test-path $script:nugetExe) -eq $false) {
        _build_step ("Downloading NuGet version " + $nugetVersion)
            Invoke-WebRequest ("https://dist.nuget.org/win-x86-commandline/v" + $nugetVersion + "/nuget.exe") -OutFile $script:nugetExe
    }
}

function __target__packages() {
    __target__downloadnuget

    _build_step "Creating NuGet packages"
        $outputFolder = join-path (Get-Location) "artifacts\packages"
        $nugetFiles = Get-ChildItem -Recurse -Include *.nuspec
        $nugetFiles | ForEach-Object {
            _exec ('& "' + $script:nugetExe + '" pack ' + $_.FullName + ' -NonInteractive -NoPackageAnalysis -OutputDirectory "' + $outputFolder + '"')
        }
}

function __target__pushmyget() {
    _build_step "Pushing packages to MyGet"
        if ($env:MyGetApiKey -eq $null) {
            Write-Host -ForegroundColor Yellow "Skipping MyGet push because environment variable 'MyGetApiKey' is not set."
            Write-Host ""
        } else {
            Get-ChildItem -Filter *.nupkg artifacts\packages | ForEach-Object {
                $cmd = '& "' + $script:nugetExe + '" push "' + $_.FullName + '" -Source https://www.myget.org/F/xunit/api/v2/package -NonInteractive -ApiKey ' + $env:MyGetApiKey
                $message = $cmd.Replace($env:MyGetApiKey, "[redacted]")
                exec $cmd $message
            }
        }
}

function __target__setversion() {
    if ($buildAssemblyVersion -ne "") {
        _build_step ("Setting assembly version: '" + $buildAssemblyVersion + "'")
            _replace "src\common\GlobalAssemblyInfo.cs" '\("99\.99\.99\.0"\)' ('("' + $buildAssemblyVersion + '")')
    }

    if ($buildSemanticVersion -ne "") {
        _build_step ("Setting semantic version: '" + $buildSemanticVersion + "'")
            _replace "src\common\GlobalAssemblyInfo.cs" '\("99\.99\.99-dev"\)' ('("' + $buildSemanticVersion + '")')
            $nugetFiles = Get-ChildItem -Recurse -Include *.nuspec
            $nugetFiles | ForEach-Object { _replace $_.FullName '99\.99\.99-dev' $buildSemanticVersion }
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

if ($PSScriptRoot -eq $null) {
    fatal "This build script requires PowerShell 3 or later."
}

Set-Location $PSScriptRoot
New-Item -Type directory -Path "artifacts\packages" -ErrorAction SilentlyContinue | out-null
New-Item -Type directory -Path "artifacts\test" -ErrorAction SilentlyContinue | out-null

$targetFunction = (Get-ChildItem ("Function:__target_" + $target.ToLowerInvariant()) -ErrorAction SilentlyContinue)
if ($targetFunction -eq $null) {
    _fatal "Unknown target '$target'"
}

_build_step "Performing pre-build verifications"
    _require dotnet "Could not find 'dotnet'. Please ensure .NET CLI Tooling is installed."
    _require msbuild "Could not find 'msbuild'. Please ensure MSBUILD.EXE v15 is on the path."
    _verify_msbuild15

& $targetFunction
