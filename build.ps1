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
if ($msbuilds.Count -eq 0) {
    throw "MSBuild could not be found in the path. Please ensure MSBuild v12 (from Visual Studio 2013) is in the path."
}

$msbuild = $msbuilds[0].Definition

if ($maxCpuCount -lt 1) {
    $maxCpuCountText = $Env:MSBuildProcessorCount
} else {
    $maxCpuCountText = ":$maxCpuCount"
}

$solutionNameArg = "/property:SolutionName=xunit.sln"
if($noXamarin) {
	$solutionNameArg = "/property:SolutionName=xunit-NoXamarin.sln"
}

$allArgs = @("xunit.msbuild", "/m$maxCpuCountText", "/nologo", "/verbosity:$verbosity", "/t:$target", "/property:RequestedVerbosity=$verbosity", $solutionNameArg, $args)
& $msbuild $allArgs
