@echo off
call tools\kvm upgrade -runtime CLR -x86
call tools\kvm install default -runtime CoreCLR -x86
call tools\kvm use default -runtime CLR -x86
call kpm restore
call kpm build src\xunit.runner.utility --configuration %1
call kpm build src\xunit.execution --configuration %1
