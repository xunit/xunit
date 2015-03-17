param(
  [string]$Configuration = "Debug"
)

Push-Location $(join-path $(split-path $MyInvocation.MyCommand.Definition) "..")

# Have to bust the cache, because of broken packages from the CLR team
Remove-Item -Recurse -Force $(join-path $env:USERPROFILE ".kpm\packages") -ErrorAction SilentlyContinue | out-null

# Make sure beta 3 is installed and in use
tools\kvm.ps1 install 1.0.0-beta3 -runtime CoreCLR -x86
tools\kvm.ps1 install 1.0.0-beta3 -runtime CLR -x86

# Restore packages and build
kpm restore
kpm build src\xunit.runner.utility.AspNet --configuration $Configuration
kpm build src\xunit.execution.AspNet --configuration $Configuration

Pop-Location
