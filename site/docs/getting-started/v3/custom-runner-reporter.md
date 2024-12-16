---
layout: default
title: "Writing a custom runner reporter for xUnit.net v3"
breadcrumb: Documentation
---

# Writing a custom runner reporter for xUnit.net v3

_Last updated: 2024 December 16_

As of version `0.5.0-pre.27`, we are now supporting custom runner reporters with xUnit.net v3. Before discussing the design of the feature in v3 Core Framework, we will review what custom runner reporters are for, and the problems with the feature as implemented in the v2 Core Framework.

## What's a runner reporter?

### Console output

Runner reporters react in realtime to test execution events. The primary purpose is to provide the console output, like you see here:

```
xUnit.net v3 In-Process Runner v0.5.0-pre.27-dev+03065987f3 (64-bit .NET 6.0.35)
  Discovering: xunit.v3.assert.tests (method display = ClassAndMethod, method display options = None)
  Discovered:  xunit.v3.assert.tests (1151 test cases to be run)
  Starting:    xunit.v3.assert.tests (parallel test collections = on [24 threads], stop on fail = off, explicit = off, seed = 309588804, culture = invariant)
  Finished:    xunit.v3.assert.tests
=== TEST EXECUTION SUMMARY ===
   xunit.v3.assert.tests  Total: 1389, Errors: 0, Failed: 0, Skipped: 0, Not Run: 0, Time: 0.172s
```

The display of the banners, test failures with stack traces, etc., is the responsibility of the runner reporter. The default runner reporter ([`DefaultRunnerReporter`](https://github.com/xunit/xunit/blob/main/src/xunit.v3.runner.common/Reporters/Builtin/DefaultRunnerReporter.cs)) is the one that provides the output you see here; or, more specifically, the message handler that `DefaultRunnerReporter` creates when asked, which in this case is [`DefaultRunnerReporterMessageHandler`](https://github.com/xunit/xunit/blob/main/src/xunit.v3.runner.common/Reporters/Builtin/DefaultRunnerReporterMessageHandler.cs).

Built into v3, we provide the default runner reporter, and a few others:

{: .table .left }
Reporter   | Purpose
---------- | -------
`json`     | Creates output in machine parseable JSON format (one message per line)
`quiet`    | Does not show banners or summary (only failures & skips)
`silent`   | Does not output anything
`teamCity` | Writes TeamCity-encoded test messages in addition to the defaults
`verbose`  | Writes starting & finishing messages in addition to the defaults

### CI support

The other thing that runner reporters do, since they have realtime access to test execution events, is report information for CI environments that can report realtime test results while your build is running. These are generally auto-enabled by detecting that you're running in the specific environment, though in the case of the TeamCity runner reporter, you can also force it with its switch. The three built-in CI environments that we support include:

* AppVeyor CI support
* Azure DevOps/VSTS CI support
* TeamCity CI support

AppVeyor and Azure DevOps support cannot be explicitly enabled, because they require several environment variables to point to HTTP API endpoints where live test results are reported.

{: .table .left }
Reporter     | Dependent environment variables
------------ | -------------------------------
AppVeyor     | `APPVEYOR_API_URL`
Azure DevOps | `VSTS_ACCESS_TOKEN`<br />`BUILD_BUILDID`<br />`SYSTEM_TEAMPROJECT`<br />`SYSTEM_TEAMFOUNDATIONCOLLECTIONURI`
TeamCity     | `TEAMCITY_PROJECT_NAME`<br />`TEAMCITY_PROCESS_FLOW_ID` (optional)

You can disable automatic CI environment support by adding the `-noAutoReporters` switch to the console runner.

### Picking a runner reporter

In v2 (and v3, prior to `0.5.0-pre.27`) you would specify these runner reporters directly via their "switch name" (i.e., `-json` for the JSON reporter), with the default help shown here:

```
Reporters (optional, choose only one)

  -json     : show progress messages in JSON format
  -quiet    : do not show progress messages
  -silent   : turns off all output messages
  -teamCity : TeamCity CI support [normally auto-enabled]
  -verbose  : show verbose progress messages
            : AppVeyor CI support [auto-enabled only]
            : Azure DevOps/VSTS CI support [auto-enabled only]
```

Now you specify `-reporter <switch>` (i.e., `-reporter json` for the JSON reporter), with the default help shown here:

```
Runner reporters (optional, choose only one)

  -reporter <option> : choose a reporter
                     :   default  - show standard progress messages
                     :   json     - show full progress messages in JSON [implies '-noLogo']
                     :   quiet    - only show failure messages
                     :   silent   - do not show any messages [implies '-noLogo']
                     :   teamCity - TeamCity CI support
                     :   verbose  - show verbose progress messages

  The following reporters will be automatically enabled in the appropriate environment.
  An automatically enabled reporter will override a manually selected reporter.
    Note: You can disable auto-enabled reporters by specifying the '-noAutoReporters' switch

    * AppVeyor CI support
    * Azure DevOps/VSTS CI support
    * TeamCity CI support
```

_**Note:** For backward compatibility reasons, the older switches will continue to be supported, although they will not be listed in the help. It's strongly encouraged that you update to the new switches to avoid future confusion from people looking at your build scripts that aren't aware of the older switches._

## Custom runner reporters in v2

While we attempted to support writing custom runner reporters in the v2 Core Framework, it had several issues.

Custom runner reporters linked against `xunit.runner.utility` and implemented `IRunnerReporter`, compiled to a DLL, and then arranged for that DLL to be in the same folder as the test project. In v2, since test projects required an external runner (like `xunit.console`), the runner was responsible for scanning for runner reporter implementations in DLLs in the test project folder and then loading them into its process (since the runner reporter runs in the same process and App Domain as the runner).

This presents the first substantial problem: with no frozen contract for `xunit.runner.utility` and no way to specify which version of `xunit.runner.utility` the runner reporter was expecting to find (since it's essentially a naked DLL sitting in the test project output folder), we frequently encountered version mismatch issues. We could not take advantage of NuGet or the .NET loader to resolve dependencies, and frequently mismatches between the runner reporter expected version of `xunit.runner.utility` vs. the version of `xunit.runner.utility` provided by the runner caused catastrophic type-related failures when trying to use the runner reporter. This caused teams to either abandon custom runner reporter support, or to version their runner dependencies separately from their unit test framework dependencies, just to ensure that version numbers lined up. Worse, if teams were trying to take advantage of multiple custom runners provided by third parties, there was no help in reconciling what versions of `xunit.runner.utility` each runner reporter was expecting, and you could easily get into a situation where two or more runner reporters were simply incompatible with each other.

The distribution of the runner reporter was also problematic, because it was not a traditional "dependency"; that is, you didn't necessarily want the test project to _link_ against the runner reporter, but only needed the DLL to be present in the output directory. This means runner reporter authors were often forced to write custom NuGet packages to ensure that this didn't cause problems for consumers.

This is all to say: we knew this design was bad and needed to be fixed.

## Custom runner reporters in v3

The fact that v3 test projects are stand-alone executables gave us an opportunity to rethink the design of custom runner reporters.

{: .note }
Custom runner reporters in v3 are only supported by the in-process console runner. That means custom runner reporters can only be selected when directly running the test project (by directly invoking the test project `.exe` or when using `dotnet run`). This also means that custom runner reporters are _**not supported**_ by first- or third-party multi-assembly runners like our console runner (`xunit.v3.runner.console`), our MSBuild runner (`xunit.v3.runner.msbuild`), or our VSTest adapter (`xunit.runner.visualstudio`, which means custom runner reporters are also not supported via `dotnet test` or Test Explorer). The first- and third-party multi-assembly runners usually only support the built-in runner reporters, if they support choosing custom runner reporters at all.

### Updates to `IRunnerReporter`

Custom runner reporters will implement `IRunnerReporter`, which is provided by `xunit.v3.runner.common`. Compared to the v2 interface, the v3 interface contains two new properties:

* `CanBeEnvironmentallyEnabled` should return `true` for a runner reporter which can be automatically enabled in the correct environment (that is, if it might ever return `true` from `IsEnvironmentallyEnabled`). This new property assists us when generating the help you saw above, to list which reporters might be automatically enabled. The canonical example of why we needed this is the TeamCity reporter: it can be explicitly enabled (that is, it returns a non-`null` value from `RunnerSwitch`), or it can be environmentally enabled. We wanted to know this, so we could included it in the environmentally enabled runner list in the help output of the in-process console runner.

* `ForceNoLogo` should return `true` to ensure that the logo text is not printed; that is: the first line of output that includes the version information, like `xUnit.net v3 In-Process Runner v0.5.0-pre.27-dev+03065987f3 (64-bit .NET 6.0.35)` shown in the example above. This is critical for reporters like the JSON reporter that want to ensure that they're only reporting machine-parseable information.

In addition, `CreateMessageHandler()` now passes a second value: an optional `IMessageSink` implementation which can be used to report diagnostic messages to.

### Linking instead of file discovery

v3 custom runner reporters need to be linked into the test project, rather than just copying a DLL into a folder. That's because the discovery mechanism for runner reporters has changed. A custom runner reporter is now registered via an assembly-level attribute:

```csharp
[assembly: RegisterRunnerReporter(typeof(MyCustomReporter))]
```

The test project must have a reference to the `MyCustomReporter` type via reference in order to perform this registration step. As this is a traditional linking scenario (usually either via NuGet or a project reference), that means the compiler can resolve any version dependencies as necessary, solving the biggest problem with v2 runner reporters.

In concert with this new registration system, we've replaced `RunnerReporterUtility.GetAvailableRunnerReporters` with `RegisteredRunnerReporters.Get`. The older API allows the developer to pass a folder to look for reporters in, whereas the new API allows the developer to pass as `Assembly` to find registrations in.

_**Note:** NuGet package authors can include MSBuild logic to automatically inject assembly registration if desired; we recommend providing a way to avoid this registration, like we do with `xunit.v3.core` as described below. To see how we do this, see [our NuGet package `.targets` file](https://github.com/xunit/xunit/blob/03065987f30548dd1a8de3a8e253995351414571/src/xunit.v3.core/Package/build/xunit.v3.core.targets#L26-L31) and the associated [C#](https://github.com/xunit/xunit/blob/03065987f30548dd1a8de3a8e253995351414571/src/xunit.v3.core/Package/content/DefaultRunnerReporters.cs), [F#](https://github.com/xunit/xunit/blob/03065987f30548dd1a8de3a8e253995351414571/src/xunit.v3.core/Package/content/DefaultRunnerReporters.fs), and [VB](https://github.com/xunit/xunit/blob/03065987f30548dd1a8de3a8e253995351414571/src/xunit.v3.core/Package/content/DefaultRunnerReporters.vb) files that get injected._

### Excluding the built-in reporters

If a developer wishes to exclude the built-in reporters, they can define the following in their project file (.csproj/.fsproj/.vbproj):

```xml
<PropertyGroup>
  <XunitRegisterBuiltInRunnerReporters>false</XunitRegisterBuiltInRunnerReporters>
</PropertyGroup>
```

This allows the developer to completely control the available runner reporter experience.

_**Note:** The default runner reporter will be whichever one is registered with a `RunnerSwitch` value of `default`. If the developer does not register one, xUnit.net will fall back to the built-in default, even if it's not registered (since a runner reporter is always required)._
