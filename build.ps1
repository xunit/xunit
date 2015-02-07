param(
    [string]$target = "Test",
    [string]$verbosity = "minimal",
    [int]$maxCpuCount = 0,
    [switch]$noXamarin = $false
)

# Kill all MSBUILD.EXE processes because they could very likely have a lock against our
# MSBuild runner from when we last ran unit tests.
get-process -name "msbuild" -ea SilentlyContinue | %{ stop-process $_.ID -force }

$msbuilds = @(get-command msbuild -ea SilentlyContinue)
if ($msbuilds.Count -gt 0) {
    $msbuild = $msbuilds[0].Definition
}
else {
    if (test-path "env:\ProgramFiles(x86)") {
        $path = join-path ${env:ProgramFiles(x86)} "MSBuild\12.0\bin\MSBuild.exe"
        if (test-path $path) {
            $msbuild = $path
        }
    }
    if ($msbuild -eq $null) {
        $path = join-path $env:ProgramFiles "MSBuild\12.0\bin\MSBuild.exe"
        if (test-path $path) {
            $msbuild = $path
        }
    }
    if ($msbuild -eq $null) {
        throw "MSBuild could not be found in the path. Please ensure MSBuild v12 (from Visual Studio 2013) is in the path."
    }
}

if ($maxCpuCount -lt 1) {
    $maxCpuCountText = $Env:MSBuildProcessorCount
} else {
    $maxCpuCountText = ":$maxCpuCount"
}

$solutionNameArg = "/property:SolutionName=xunit.vs2013.sln"
if($noXamarin) {
    $solutionNameArg = "/property:SolutionName=xunit.vs2013.NoXamarin.sln"
}

$allArgs = @("xunit.msbuild", "/m$maxCpuCountText", "/nologo", "/verbosity:$verbosity", "/t:$target", "/property:RequestedVerbosity=$verbosity", $solutionNameArg, $args)
& $msbuild $allArgs
