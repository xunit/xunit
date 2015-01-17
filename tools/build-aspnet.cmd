@echo off
call tools\kvm install 1.0.0-beta2 -runtime CLR -x86
call tools\kvm install 1.0.0-beta2 -runtime CoreCLR -x86
call tools\kvm use 1.0.0-beta2 -runtime CLR -x86
call kpm restore
call kpm build src\xunit.runner.utility.AspNet --configuration %1
call kpm build src\xunit.execution.AspNet --configuration %1
