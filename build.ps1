param(
    [string]$target = "Test",
    [string]$verbosity = "minimal"
)

$msbuilds = @(get-command msbuild -ea SilentlyContinue)
if ($msbuilds.Count -eq 0) {
    $msbuild = join-path $env:windir "Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
} else {
    $msbuild = $msbuilds[0].Definition
}

$allArgs = @("xunit.msbuild", "/m", "/nologo", "/verbosity:$verbosity", "/t:$target", $args)
& $msbuild $allArgs
