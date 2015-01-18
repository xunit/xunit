@echo off
pushd %~dp0\..

rem Have to bust the cache, because of broken packages from the CLR team
rmdir /q /s %USERPROFILE%\.kpm\packages > nul 2>&1

rem Make sure beta 2 is installed and in use
powershell -NoProfile -NonInteractive -ExecutionPolicy RemoteSigned tools\kvm.ps1 install 1.0.0-beta2 -runtime CLR -x86
powershell -NoProfile -NonInteractive -ExecutionPolicy RemoteSigned tools\kvm.ps1 install 1.0.0-beta2 -runtime CoreCLR -x86
powershell -NoProfile -NonInteractive -ExecutionPolicy RemoteSigned tools\kvm.ps1 use 1.0.0-beta2 -runtime CLR -x86

rem Restore packages and build
call kpm restore
call kpm build src\xunit.runner.utility.AspNet --configuration %1
call kpm build src\xunit.execution.AspNet --configuration %1

popd
