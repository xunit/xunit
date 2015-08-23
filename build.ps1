param(
    [string]$target = "Test",
    [string]$verbosity = "minimal",
    [int]$maxCpuCount = 0
)

# Kill all MSBUILD.EXE processes because they could very likely have a lock against our
# MSBuild runner from when we last ran unit tests.
get-process -name "msbuild" -ea SilentlyContinue | %{ stop-process $_.ID -force }

if (test-path "env:\ProgramFiles(x86)") {
    $path = join-path ${env:ProgramFiles(x86)} "MSBuild\14.0\bin\MSBuild.exe"
    if (test-path $path) { $msbuild = $path }
}
if ($msbuild -eq $null) {
    $path = join-path $env:ProgramFiles "MSBuild\14.0\bin\MSBuild.exe"
    if (test-path $path) { $msbuild = $path }
}
if ($msbuild -eq $null) {
    throw "Could not find MSBuild v14. Please install it (or Visual Studio 2015)."
}

if ($maxCpuCount -lt 1) {
    $maxCpuCountText = $Env:MSBuildProcessorCount
} else {
    $maxCpuCountText = ":$maxCpuCount"
}

$allArgs = @("xunit.msbuild", "/m$maxCpuCountText", "/nologo", "/verbosity:$verbosity", "/t:$target", "/property:RequestedVerbosity=$verbosity", "/property:SolutionName=xunit.vs2015.sln", $args)
& $msbuild $allArgs
