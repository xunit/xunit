@echo off
rem Have to blow away the package cache because the CLR team can't get its shit together and publish sane packages
rmdir /q /s %USERPROFILE%\.kpm\packages

call tools\kvm install 1.0.0-beta1 -runtime CLR -x86
call tools\kvm install 1.0.0-beta1 -runtime CoreCLR -x86
call tools\kvm use 1.0.0-beta1 -runtime CLR -x86
call kpm restore
call kpm build src\xunit.runner.utility.AspNet --configuration %1
call kpm build src\xunit.execution.AspNet --configuration %1
