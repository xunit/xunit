---
layout: default
title: v3 Alpha Status
breadcrumb: Documentation
---

# Current state of the xUnit.net v3 alpha

## As of: 2024 July 5 (`0.2.0-pre.7`)

The purpose of this document is to give a general state of the alpha package releases of xUnit.net v3.
Note that there have not yet been any "stable alpha" releases to NuGet, so this is a snapshot of a moment
in time.

The packages from CI builds are available on feedz.io. For more information on setting up for CI packages,
see the [Using CI Builds doc page](using-ci-builds).

## Table of contents

* [Big changes from v2](#v2-changes)
* [Add nuget.config with CI package feed URL](#feed)
* [Migrating a test project from v2 to v3](#migrating-from-v2)
* [Creating a new v3 test project](#creating-test-project)
* [Running a v3 test project](#running-test-project)
* [Overriding the entry point](#overriding-entry-point)
* [Known issues](#known-issues)


## <span id="v2-changes"></span>Big changes from v2

This list highlights the major architectural changes in v3.

### New minimum runtime requirements

For unit test authors, we have bumped up the minimum runtime requirements to match
with our move to `netstandard2.0`. Today, our supported runtimes include:

* .NET Framework 4.7.2 (or later)
* .NET 6 (or later)

Also new for v3: Mono is officially supported on Linux and macOS for .NET Framework
test projects. While it did often work with v1 and v2, we do officially test and
verify with it now.

The full list of planned runtimes for v3 can be found in
[this issue](https://github.com/xunit/xunit/issues/2330).

### Unit test projects are applications now

With xUnit.net v1 and v2, unit test projects were class library projects; that is,
when compiled, they always generated `.dll` files, which relied upon an external
runner to run.

This is a design that dates back to the beginnings of .NET Framework, long before
.NET Core came into being. Even then it had some unfortunate downsides. Let's take
a look at a couple.

One example is library dependency management: if the runner loads your test assembly
into the same process as itself, and both pieces of code wish to use a library, there
was a "first one wins" conflict, which meant your unit test always lost. In .NET
Framework, the workaround was App Domains, which is not available with .NET Core
(and sometimes test code didn't run properly with App Domains). Additionally, the
assembly dependency resolution system in .NET Core is exceptionally complex and
not designed to consumed directly (especially as it pertained to un-managed
dependencies, like Win32 DLLs), so it was a frequent problem running .NET Core
tests in-line with a runner.

Another example is that there are some things that can only be chosen on a process
basis, not an App Domain basis. One category of those things would be where .NET APIs
are just thin wrappers around Win32 functionality, where there's no App Domain awareness.
One common place that bit people with testing is `Directory.SetCurrentDirectory`. If a
runner had loaded multiple test assemblies to run in parallel, and they each call that API,
they are mutating a piece of shared state, which can cause unpredictable failures.
Similarly, with .NET Framework, the "chosen" version of the .NET Framework (as well
as 32- vs. 64-bit-ness) is a decision made by the process, which in the case of stand alone
runners means the <i>runner</i> chooses that rather than the unit test. This became such a
significant issue that we shipped at least a dozen versions of the console runner at a time:
the cross-product of 32- vs. 64-bit and .NET 4.5.2 vs. 4.6 vs. 4.6.1, etc.

The solution to all these problems is that unit tests should be run in their own process.
It lets us leverage the existing assembly resolution logic without needing anything
special, and offers a better level of isolation from one test project to another when
running in parallel.

_**Note:** We will continue to ship several versions of the v3 console runner as it's still
able to run v1 and v2 tests, but the choice of .NET Framework version and bitness is no
longer applicable when running v3 tests. In fact, the v3 console runner (which is only
shipped built against .NET Framework) can run v3 .NET Core projects, since they are run
out of process._

### netstandard2.0 is the new norm

In v2, we separated two libraries: `xunit.core.dll` and `xunit.execution.*.dll`. The purpose
of this separation was two-fold: to isolate the code used to write tests and the code used
to run those tests; to hide the fact that while `core` targeted `netstandard1.1`, `execution`
was forced to ship framework-specific DLLs.

With v3, these two libraries have been collapsed into `xunit.v3.core.dll` and the target is now
`netstandard2.0`. This will primarily benefit extensibility authors who previously had to
choose whether to extend `core` and/or `execution`, and more specifically, had to ship
multi-targeted libraries to match whichever runtimes they wanted to support.

Note that currently the `xunit.v3.core` (and `xunit.v3`) NuGet package shows target frameworks
of `net472` and `net6.0` because of the in-process runner requirement. Developers who extend
xUnit.net will use the `xunit.v3.extensibility.core` NuGet package instead, which is
single-targeted against `netstandard2.0`. Extensibility authors will no longer need to ship
multi-targeted NuGet packages.


## <span id="feed"></span>Add nuget.config with CI package feed URL

The CI builds are hosted on [feedz.io](https://feedz.io/org/xunit/repository/xunit/search).
In order to download packages from this feed, you need to configure it with a `nuget.config`
file. Regardless of whether you're planning to upgrade an existing project from v2 to v3,
or start a new v3 project, you must take this step first or else package restore of the v3
packages will fail.

In your solution folder, create a file named `nuget.config` and add the following contents:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget" value="https://api.nuget.org/v3/index.json" />
    <add key="xunit-ci" value="https://f.feedz.io/xunit/xunit/nuget/index.json" />
  </packageSources>
</configuration>
```

_**Note:** You may need to restart your IDE for it to pick up these changes._

If you already have a `nuget.config` file, you can simply merge the `<add key="xunit-ci" ...>`
line into it.


## <span id="migrating-from-v2"></span>Migrating a test project from v2 to v3

The following is a quick list of changes that are needed when moving a test project from v2
to v3. Your project may require additional changes. Note that it's generally expected
that unit test projects should "just work" when migrating from v2 to v3; porting
extensibility libraries and/or runners from v2 to v3 is beyond the scope of this document
at this time (in addition to the fact that the APIs being still very much under development).

### Update NuGet package references

Change the following package references:

{: .table .latest }
| v2 package                                                      | v3 package                                       |
| --------------------------------------------------------------- | ------------------------------------------------ |
| `xunit`                                                         | `xunit.v3`                                       |
| `xunit.abstractions`                                            | Remove, no longer required                       |
| `xunit.analyzers`                                               | Unchanged                                        |
| `xunit.assert`                                                  | `xunit.v3.assert`                                |
| `xunit.assert.source`                                           | `xunit.v3.assert.source`                         |
| `xunit.console`                                                 | Remove, no longer supported                      |
| `xunit.core`                                                    | `xunit.v3.core`                                  |
| `xunit.extensibility.core`<br />`xunit.extensibility.execution` | `xunit.v3.extensibility.core` (*)                |
| `xunit.runner.console`                                          | xunit.v3.runner.console                          |
| `xunit.runner.msbuild`                                          | xunit.v3.runner.msbuild                          |
| `xunit.runner.reporters`<br />`xunit.runner.utility`            | `xunit.v3.runner.utility` (*)                    |
| `xunit.runner.visualstudio`                                     | Make sure to pick up a pre-release versioned 3.x |

_**Note:** In some cases multiple libraries/packages were merged together into a single new library/package, as denoted in the table above with (*)._

If you want to use the MSBuild runner, we now ship both a .NET Framework and .NET Core/.NET version
in the same package. It will dynamically select the correct version depending on whether you use
the .NET Framework MSBuild or the .NET MSBuild (via `dotnet build` or `dotnet msbuild`). However,
the .NET version only supports v3 test projects. If you need to still run v1 and/or v2 test projects,
you must use the .NET Framework version. (Mono ships with the .NET Framework version of MSBuild,
so all comments about .NET Framework also apply to Mono.)

### Convert to executable project

Update your project file (i.e., `.csproj`) and change `OutputType` from `Library` to
`Exe`. You may need to add `OutputType` if it's not present, since `Library` is the
default value:

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
</PropertyGroup>
```

### Update target framework

There are new [minimum target framework versions](https://github.com/xunit/xunit/issues/2330); make
sure to update your target framework(s) if you're currently targeting something that's too old.

## <span id="creating-test-project"></span>Creating a new v3 test project

Since there is no project template yet for xUnit.net v3, you should create a project using
`dotnet new console` from the .NET SDK command line tool (or create a console project from within
your favored IDE). We currently support C#, F#, and VB.NET, targeting .NET Framework 4.7.2+ and/or
.NET 6+.

After creation, edit your project file to make it look something like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit.v3" Version="0.2.0-pre.7" />

    <!-- To support 'dotnet test' and Test Explorer, add:
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.10.0" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.0.0-pre.10" />
    -->

    <!-- For the console runner, add:
    <PackageReference Include="xunit.v3.runner.console" Version="0.2.0-pre.7" />
    -->

    <!-- For the MSBuild runner, add:
    <PackageReference Include="xunit.v3.runner.msbuild" Version="0.2.0-pre.7" />
    -->
  </ItemGroup>

</Project>
```


## <span id="running-test-project"></span>Running a v3 test project

### Running test projects directly

As a new to v3 feature, you can directly run a test project. No external runner is
required, as we've shipped an in-process runner for you already:

* For projects targeting .NET Framework, you can directly run the executable, within
your IDE or from the command line. If you are on Linux or macOS, you may need to invoke
the executable through the `mono` command line.
* For projects targeting .NET 6+, you can use `dotnet run` to run your project from
the command line (or run it from within your IDE).

The test project has a command line just like the console runner, which you can invoke
by adding `-?` to the end of the executable invocation (and if you're using `dotnet run`,
don't forget to put a `--` just in front of the arguments you want to pass to the runner,
since that's how `dotnet run` differentiates command line options for itself vs. those
intended for your program).

Examples:

* `MyProject\bin\Debug\net472\MyTests.exe -?` (Windows)
* `mono MyProject/bin/Debug/net472/MyTests.exe -?` (Linux/macOS)
* `dotnet run --project MyProject -- -?`

### Running test project with first and third party runners

You can run your v3 test projects with first and third party test runners that have
been developed to do so (by using `xunit.v3.runner.utility`). We ship NuGet packages
for our console runner (`xunit.v3.runner.console`) and our MSBuild runner
(`xunit.v3.runner.msbuild`). These work exactly like their v2 counterparts. We have
included a new .NET Core/.NET version of our MSBuild runner, so it can be used with
`dotnet build` and/or `dotnet msbuild`; however, this version of the MSBuild runner
can only run v3 test projects. To be able to run v1 or v2 test projects, you must
stick with the .NET Framework of the MSBuild runner, run via `msbuild`.

We also are making pre-release builds of `xunit.runner.visualstudio` available for
developers who wish to run their tests via `dotnet test`, Test Explorer in Visual Studio
2022, or any other IDE which supports VSTest. Make sure to use one of the `3.0.0-pre`
builds for this functionality, as the builds from the `2.x` tree are still only
capable of running v1 and v2 tests.


## <span id="overriding-entry-point"></span>Overriding the entry point


Since unit test projects are programs, that means they need a `Main` method. However, you
didn't write one, so where did it come from?

We inject one. Here are the three versions:

### C#

```csharp
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class AutoGeneratedEntryPoint
{
    public static async global::System.Threading.Tasks.Task<int> Main(string[] args)
    {
        return await global::Xunit.Runner.InProc.SystemConsole.ConsoleRunner.Run(args);
    }
}
```

### F#

```fsharp
module AutoGeneratedEntryPoint

[<EntryPoint>]
[<global.System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>]
let main args =
    global.Xunit.Runner.InProc.SystemConsole.ConsoleRunner.Run(args).GetAwaiter().GetResult()
```

### VB

```vb
<Global.System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage>
Public Class AutoGeneratedEntryPoint
    Public Shared Function Main(args As String()) As Integer
        Return Global.Xunit.Runner.InProc.SystemConsole.ConsoleRunner.Run(args, Nothing, Nothing, Nothing).GetAwaiter().GetResult()
    End Function
End Class
```

If you want to provide your own entry point (for example, because you want to run ASP.NET
Core initialization code before running your tests), you can set the following property
in your project file:

```xml
<PropertyGroup>
  <XunitAutoGeneratedEntryPoint>false</XunitAutoGeneratedEntryPoint>
</PropertyGroup>
```

Once you've done this, you're responsible for defining the `Main` method for your application,
and then calling `ConsoleRunner.Run` to get things started.


## <span id="known-issues"><span>Known issues

The best place to keep track of the ongoing work is the [roadmap](https://github.com/xunit/xunit/issues/2133).

### Old-style .NET Framework C#/VB project issues.

In general, new (SDK-style) projects are preferred and supported. Old-style project files for .NET
Framework C# and VB should work as well; if you start by creating a .NET Framework Console
application project, make sure to delete the generated entry point (`Program.cs` for C# and
`Module1.vb` for VB) and make sure to remove any `<StartupObject>` properties that might
exist in your project file, as the entry point is automatically provided by the NuGet package.
