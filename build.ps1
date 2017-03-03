param(
    [string]$target = "Build",
    [string]$verbosity = "minimal",
    [int]$maxCpuCount = 0
)

if ((get-command msbuild -ErrorAction SilentlyContinue) -eq $null) {
    write-host -ForegroundColor Red "error: Could not find MSBUILD. Please ensure MSBUILD.EXE v15 is on the path."
    exit 1
}

$version = & msbuild /nologo /ver

if (-not $version.StartsWith("15.")) {
    write-host -ForegroundColor Red "error: Unexpected MSBUILD version $version. Please ensure MSBUILD.EXE v15 is on the path."
    exit 1
}

if ($maxCpuCount -lt 1) {
    $maxCpuCountText = $Env:MSBuildProcessorCount
} else {
    $maxCpuCountText = ":$maxCpuCount"
}

# Kill all MSBUILD.EXE processes because they could very likely have a lock against our
# MSBuild runner from when we last ran unit tests.
get-process -name "msbuild" -ea SilentlyContinue | %{ stop-process $_.ID -force }

$allArgs = @("xunit.msbuild", "/nr:false", "/m$maxCpuCountText", "/nologo", "/verbosity:$verbosity", "/target:$target", $args)
& msbuild $allArgs
