@echo off
call tools\kvm install 1.0.0-beta1 -runtime CLR -x86
call tools\kvm install 1.0.0-beta1 -runtime CoreCLR -x86
call tools\kvm use 1.0.0-beta1 -runtime CLR -x86
call kpm restore
call kpm build src\xunit.runner.utility --configuration %1
call kpm build src\xunit.execution --configuration %1
