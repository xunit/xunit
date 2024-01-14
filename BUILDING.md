# Building xUnit.net

The primary build system for xUnit.net is done via command line, and officially supports Windows and Visual Studio.

# Pre-Requisites

You will need the following software installed:

* .NET Framework 4.7.2 or later (part of the Windows OS)
* [Visual Studio 2022 or later](https://visualstudio.microsoft.com/vs/)
  * ".NET desktop development" workload
  * Additional components:
    * .NET Framework development tools for 3.5
    * .NET Framework targeting packs for 4.5.2, 4.6, 4.6.1, 4.6.2, 4.7, 4.7.1, 4.7.2, 4.8, 4.8.1
    * Windows 10 SDK 10.0.19041.0
    * F# desktop language support
* [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
* [.NET 6.0 Runtime](https://dotnet.microsoft.com/download/dotnet/6.0)
* [.NET Core 2.0 Runtime](https://dotnet.microsoft.com/download/dotnet/2.0)
* PowerShell (or [PowerShell Core](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-windows))

Ensure that you have configured PowerShell to be able to run local unsigned scripts (either by running
`Set-ExecutionPolicy -Scope CurrentUser RemoteSigned` from within PowerShell, or by launching PowerShell with the
`-ExecutionPolicy RemoteSigned` command line switch).

# Command-Line Build

1. Open PowerShell (or PowerShell Core). (To ensure MSBUILD is on the path, you may wish to start with "Developer Powershell for VS" shortcut.)

1. From the root folder of the source repo, this command will build the code and run all tests:

    `./build`

    To build a specific target (or multiple targets):

    `./build [target [target...]]`

    The common targets (case-insensitive) include:

    * `Restore`: Perform package restore
    * `Build`: Build the source
    * `Test`: Run all unit tests
    * `Packages`: Create NuGet packages
    * `FormatSource`: Formats the source code (use this if the build fails because of improper formatting)

# Failing to build in Visual Studio

Visual Studio does not perform the same package restore as MSBuild, which may cause your build to fail with errors like this:

```
C:\...\Microsoft.Common.CurrentVersion.targets(1229,5): error MSB3644:
The reference assemblies for .NETFramework,Version=v4.0 were not found.
To resolve this, install the Developer Pack (SDK/Targeting Pack)
for this framework version or retarget your application. You can
download .NET Framework Developer Packs at
https://aka.ms/msbuild/developerpacks
```

If this happens, you can run `./build restore` from the root folder of the source repo. This will ensure that the .NET Framework reference assemblies are properly restored and referenced, and that should fix any further build issues with Visual Studio.
