param(
  [string]$Configuration = "Debug"
)

Push-Location $(join-path $(split-path $MyInvocation.MyCommand.Definition) "..")

# Have to bust the cache, because of broken packages from the CLR team
Remove-Item -Recurse -Force $(join-path $env:USERPROFILE ".dnx\packages") -ErrorAction SilentlyContinue | out-null

# Make sure beta 3 is installed and in use
tools\dnvm.ps1 install latest -runtime CoreCLR -arch x86
tools\dnvm.ps1 install latest -runtime CLR -arch x86

# Restore packages and build
dnu restore
dnu build src\xunit.runner.utility.dnx --configuration $Configuration
dnu build src\xunit.execution.dnx --configuration $Configuration

Pop-Location
