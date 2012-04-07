@echo off
if "%1" == "" goto BuildDefault
goto BuildTarget

:BuildDefault
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe xunit.msbuild /t:Build
goto End

:BuildTarget
%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe xunit.msbuild /t:%*

:End
