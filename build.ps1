param(
    [string]$target = "Test"
)

$msbuilds = @(get-command msbuild -ea SilentlyContinue)
if ($msbuilds.Count -eq 0) {
    $msbuild = join-path $env:windir "Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
} else {
    $msbuild = $msbuilds[0].Definition
}

$allArgs = @("xunit.msbuild", "/m", "/nologo", "/verbosity:minimal", "/t:$target", $args)
& $msbuild $allArgs
