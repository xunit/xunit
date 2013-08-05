param(
    [string]$target = "Test"
)

$msbuilds = @(get-command msbuild)
if ($msbuilds.Count -eq 0) {
    $msbuild = join-path $env:windir "Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
} else {
    $msbuild = $msbuilds[0].Definition
}

$cmdline = new-object System.Collections.ArrayList
$cmdline.Add("xunit.msbuild") | out-null
$cmdline.Add("/t:" + $target) | out-null
$args | %{ $cmdline.Add($_) | out-null }

& $msbuild $cmdline
