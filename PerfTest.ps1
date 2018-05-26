Write-Host -Foreground Yellow -NoNewLine "vstest.console.exe (net452): "
Write-Host (Measure-Command { & "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" /TestAdapterPath:src\xunit.runner.visualstudio\bin\Debug\net452 /ListTests /Settings:test\test.harness\Settings.runsettings test\test.harness\bin\Debug\net452\test.harness.dll }).TotalMilliseconds

Write-Host -Foreground Yellow -NoNewLine "xunit.console.exe (net452):  "
Write-Host (Measure-Command { & src\xunit.console\bin\Debug\net452\xunit.console.exe test\test.harness\bin\Debug\net452\test.harness.dll -class Foo }).TotalMilliseconds

Write-Host -Foreground Yellow -NoNewLine "dotnet vstest (netcore1.0):  "
Write-Host (Measure-Command { & dotnet vstest /Framework:FrameworkCore10 /ListTests /Settings:test\test.harness\Settings.runsettings test\test.harness\bin\Debug\netcoreapp1.0\test.harness.dll }).TotalMilliseconds

Write-Host -Foreground Yellow -NoNewLine "dotnet xunit (netcore1.0):   "
Write-Host (Measure-Command { & dotnet src\xunit.console\bin\Debug\netcoreapp1.0\xunit.console.dll test\test.harness\bin\Debug\netcoreapp1.0\test.harness.dll -class Foo }).TotalMilliseconds
