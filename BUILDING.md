# Building xUnit.net

The primary build system for xUnit.net is done via command line, and officially supports Linux and Windows. Users
running macOS can generally follow the Linux instructions (while installing the macOS equivalents of the dependencies).

# Pre-Requisites

You will need the following software installed (regardless of OS):

* [.NET SDK 5.0](https://dotnet.microsoft.com/download/dotnet/5.0)
* [.NET Core Runtime 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
* [.NET Core Runtime 2.1](https://dotnet.microsoft.com/download/dotnet-core/2.1)
* [git](https://git-scm.com/downloads)

## Linux Pre-Requisites

Linux users will additionally need:

* [Mono](https://www.mono-project.com/download/stable/) 6.12+ w/ MSBuild 16.6+
* [bash](https://www.gnu.org/software/bash/)

## Windows Pre-Requisites

Windows users will additionally need:

* .NET Framework 4.7.2 or later (part of the Windows OS)
* PowerShell (or [PowerShell Core](https://docs.microsoft.com/en-us/powershell/scripting/install/installing-powershell-core-on-windows?view=powershell-6))

Ensure that you have configured PowerShell to be able to run local unsigned scripts (either by running
`Set-ExecutionPolicy RemoteSigned` from within PowerShell, or by launching PowerShell with the
`-ExecutionPolicy RemoteSigned` command line switch).

# Command-Line Build

1. **Linux users:** Open a terminal to your favorite shell.

    **Windows users:** Open PowerShell (or PowerShell Core).

1. From the root folder of the source repo, this command will build the code & run all tests:

    `./build`

    To build a specific target (or multiple targets):

    `./build [target [target...]]`

    The common targets (case-insensitive) include:

    * `Restore`: Perform package restore
    * `Build`: Build the source
    * `Test`: Run all unit tests
    * `TestCore`: Run all unit tests (.NET Core)
    * `TestFx`: Run all unit tests (.NET Framework)
    * `Packages`: Create NuGet packages

    You can get a list of options:

    `./build --help`
