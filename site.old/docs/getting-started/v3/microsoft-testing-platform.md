---
layout: default
title: "Microsoft Testing Platform support in xUnit.net v3"
breadcrumb: Documentation
---

# Microsoft Testing Platform support in xUnit.net v3

_Last updated: 2024 December 16_

Starting with build `0.4.0-pre.10`, we have added support for the new [Microsoft Testing Platform](https://learn.microsoft.com/dotnet/core/testing/unit-testing-platform-intro) natively into xUnit.net v3.

## What is Microsoft Testing Platform?

VSTest has been the underlying driver behind `dotnet test` and Test Explorer (and `vstest.console` and Test View before them) since it first launched in Visual Studio 2010. The new Microsoft Testing Platform aims to replace those with a new engine that is modernized, streamlined, performs better, and offers much greater extensibility for test framework authors.

Much like xUnit.net v3—and for many of the same reasons—test projects for Microsoft Testing Platform are standalone executables. When a Microsoft Testing Platform test project is compiled, it can then be run directly (typically by invoking the already built executable, or using `dotnet run` to both build and run). This allows for a streamlined experience where the produced executable is all that's needed to run the tests.

The xUnit.net integration with Microsoft Testing Platform comes at three levels:

1. You can replace the default xUnit.net command line experience with the Microsoft Testing Platform command line experience ([&#x1F517;](#enabling-the-command-line-experience))
2. You can run tests with the new Microsoft Testing Platform integrated `dotnet test` ([&#x1F517;](#enabling-the-dotnet-test-experience))
3. You can run tests with the new Microsoft Testing Platform integrated Test Explorer ([&#x1F517;](#enabling-the-test-explorer-experience))

Unlike our support for VSTest, our support for Microsoft Testing Platform is built natively into xUnit.net v3. If you want to rely solely on Microsoft Testing Platform support, you can remove the package references to `xunit.runner.visualstudio` and `Microsoft.NET.Test.Sdk`. However, for backward compatibility reasons, we recommend you leave these in place, because as of the writing of this document, third party runners (like Resharper/Rider, CodeRush, and Visual Studio Code) still rely on VSTest to be able to run xUnit.net tests. Once all runners can support Microsoft Testing Platform, then we'll be able to deprecate `xunit.runner.visualstudio`. Supporting VSTest is separate from (and does not interfere with) our support for Microsoft Testing Platform.


## Enabling the command line experience

By default, xUnit.net v3 projects have a native command line experience that is similar to our console runner command line experience.

If you `dotnet run` your test project, you should see something like this (examples using our test project from the [Getting Started with the command line](cmdline) documentation):

```
$ dotnet run
xUnit.net v3 In-Process Runner v1.0.0+5b41c61aa1 (64-bit .NET 8.0.11)
  Discovering: MyFirstUnitTests
  Discovered:  MyFirstUnitTests
  Starting:    MyFirstUnitTests
    MyFirstUnitTests.UnitTest1.FailingTest [FAIL]
      Assert.Equal() Failure: Values differ
      Expected: 5
      Actual:   4
      Stack Trace:
        UnitTest1.cs(14,0): at MyFirstUnitTests.UnitTest1.FailingTest()
    MyFirstUnitTests.UnitTest1.MyFirstTheory(value: 6) [FAIL]
      Assert.True() Failure
      Expected: True
      Actual:   False
      Stack Trace:
        UnitTest1.cs(28,0): at MyFirstUnitTests.UnitTest1.MyFirstTheory(Int32 value)
  Finished:    MyFirstUnitTests
=== TEST EXECUTION SUMMARY ===
   MyFirstUnitTests  Total: 5, Errors: 0, Failed: 2, Skipped: 0, Not Run: 0, Time: 0.076s
```

If you want to replace the xUnit.net native command line experience with the Microsoft Testing Platform command line experience, add the following property to your project file (.csproj/.fsproj/.vbproj):

```xml
<PropertyGroup>
  <UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>
</PropertyGroup>
```

Now, using `dotnet run`, you should see:

```
$ dotnet run
xUnit.net v3 Microsoft.Testing.Platform Runner v1.0.0+5b41c61aa1 (64-bit .NET 8.0.11)

failed MyFirstUnitTests.UnitTest1.MyFirstTheory(value: 6) (0ms)
  Assert.True() Failure
  Expected: True
  Actual:   False
    at MyFirstUnitTests.UnitTest1.MyFirstTheory(Int32 value) in UnitTest1.cs:28
failed MyFirstUnitTests.UnitTest1.FailingTest (2ms)
  Assert.Equal() Failure: Values differ
  Expected: 5
  Actual:   4
    at MyFirstUnitTests.UnitTest1.FailingTest() in UnitTest1.cs:14

Test run summary: Failed! - bin\Debug\net8.0\MyFirstUnitTests.dll (net8.0|x64)
  total: 5
  failed: 2
  succeeded: 3
  skipped: 0
  duration: 142ms
```

Using the Microsoft Testing Platform command line experience will give you a familiar UX if you frequently use other Microsoft Testing Platform integrated test frameworks, like [MSTest 3.6](https://github.com/microsoft/testfx/blob/main/docs/Changelog.md#3.6.0) or [TUnit](https://thomhurst.github.io/TUnit/).

The command line switches are different between the two platforms; type `dotnet run -- -?` to see them. The table below offers a rough mapping between the xUnit.net native command line option and the equivalent Microsoft Testing Platform command line option:

{: .table .left .smaller }
xUnit.net                             | Microsoft Testing Platform                                                | Activity
------------------------------------- | ------------------------------------------------------------------------- | --------
`:<seed>`                             | `--seed <seed>`                                                           | Set the randomization seed
`path/to/configFile.json`             | `--xunit-config-filename path/to/configFile.json`                         | Set the configuration file (defaults to `xunit.runner.json`)
`-assertEquivalentMaxDepth <option>`  | `--assert-equivalent-max-depth <option>`                                  |
`-class "name"`                       | `--filter-class "name"`<sup>1</sup>                                       | Run all methods in a given test class
`-class- "name`                       | `--filter-not-class "name"`<sup>1</sup>                                   | Do not run any methods in the given test class
`-ctrf <filename>`                    | `--report-ctrf --report-ctrf-filename <filename>`<sup>2</sup>             | Enable generating CTRF (JSON) report
`-culture <option>`                   | `--culture <option>`                                                      | Set the execution culture
`-diagnostics`                        | `--xunit-diagnostics on`<sup>3</sup>                                      | Display diagnostic messages
`-explicit <option>`                  | `--explicit <option>`                                                     | Change the way explicit tests are handled
`-failSkips`                          | `--fail-skips on`                                                         | Treat skipped tests as failed
`-failSkips-`                         | `--fail-skips off`                                                        | Treat skipped tests as skipped
`-failWarns`                          | `--fail-warns on`                                                         | Treat passing tests with warnings as failed
`-failWarns-`                         | `--fail-warns off`                                                        | Treat passing tests with warnings as passed
`-internalDiagnostics`                | `--xunit-internal-diagnostics on`<sup>3</sup>                             | Display internal diagnostic messages
`-html <filename>`                    | `--report-xunit-html --report-xunit-html-filename <filename>`<sup>2</sup> | Enable generating xUnit.net HTML report
`-jUnit <filename>`                   | `--report-junit --report-junit-filename <filename>`<sup>2</sup>           | Enable generating JUnit (XML) report
`-longRunning <seconds>`              | `--long-running <seconds>`                                                | Enable long running (hung) test detection
`-maxThreads <option>`                | `--max-threads <option>`                                                  | Set maximum thread count for collection parallelization
`-method "name"`                      | `--filter-method "name"`<sup>1</sup>                                      | Run a given test method
`-method- "name"`                     | `--filter-not-method "name"`<sup>1</sup>                                  | Do not run a given test method
`-methodDisplay <option>`             | `--method-display <option>`                                               | Set default test display name
`-methodDisplayOptions <option>`      | `--method-display-options <option>`<sup>4</sup>                           | Alters the default test display name
`-namespace "name"`                   | `--filter-namespace "name"`<sup>1</sup>                                   | Run all methods in the given namespace
`-namespace- "name"`                  | `--filter-not-namespace "name"`<sup>1</sup>                               | Do not run any methods in the given namespace
`-noAutoReporters`                    | `--auto-reporters off`<sup>3</sup>                                        | Do not allow reporters to be auto-enabled by environment
`-nUnit <filename>`                   | `--report-nunit --report-nunit-filename <filename>`<sup>2</sup>           | Enable generating NUnit (v2.5 XML) report
`-parallel <option>`                  | `--parallel <option>`                                                     | Change test parallelization
`-parallelAlgorithm <option>`         | `--parallel-algorithm <option>`                                           | Change the parallelization algorithm
`-preEnumerateTheories`               | `--pre-enumerate-theories on`<sup>3</sup>                                 | Turns on theory pre-enumeration
`-printMaxEnumerableLength <option>`  | `--print-max-enumerable-length <option>`                                  |
`-printMaxObjectDepth <option>`       | `--print-max-object-depth <option>`                                       |
`-printMaxObjectMemberCount <option>` | `--print-max-object-member-count <option>`                                |
`-printMaxStringLength <option>`      | `--print-max-string-length <option>`                                      |
`-showLiveOutput`                     | `--show-live-output on`<sup>3</sup>                                       | Turns on live reporting of test output (from ITestOutputHelper)
`-stopOnFail`                         | `--stop-on-fail on`<sup>3</sup>                                           | Stop running tests after the first test failure
`-trait "name=value"`                 | `--filter-trait "name=value"`<sup>1</sup>                                 | Run all methods with a given trait value
`-trait- "name=value"`                | `--filter-not-trait "name=value"`<sup>1</sup>                             | Do not run any methods with a given trait value
`-trx <filename>`                     | `--report-xunit-trx --report-xunit-trx-filename <filename>`<sup>2</sup>   | Enable generating xUnit.net TRX report
`-xml <filename>`                     | `--report-xunit --report-xunit-filename <filename>`<sup>2</sup>           | Enable generating xUnit.net (v2+ XML) report

_**Notes:**_

> _<sup>1</sup> Filter options in the xUnit.net command line interface must be specified one at a time, repeating the filter switch each time. With the Microsoft Testing Platform command line interface, multiple filters of the same kind can be specified with just a single switch. For example, `-class Foo -class Bar` in the xUnit.net command line interface can be expressed as `--filter-class Foo Bar` in the Microsoft Testing Platform command line interface._
>
> _<sup>2</sup> Unlike in the xUnit.net command line experience, providing a filename with Microsoft Testing Platform for reports is optional, and will default to a name that includes the current username, computer name, and the current date & time when the report was run. If you specify the report filename in Microsoft Testing Platform, it must be a filename only without path components. All reports are output to the results folder, which defaults to `TestResults` under the output folder (and can be overridden with `--results-directory <directory>`)._
>
> _<sup>3</sup> These options in the xUnit.net command line interface can only be specified in one direction (either on or off, depending on the switch). The Microsoft Testing Platform command line interface allows both `on` and `off` command line options, which offers more flexibility (and in the `-?` help, describes which value is the default)._
>
> _<sup>4</sup> The method display options switch on the xUnit.net command line interface must be expressed as multiple values pushed together with commas. The Microsoft Testing Platform command line interface allows the individual values to be specified without the comma separators. For example, `-methodDisplayOptions replacePeriodWithComma,useEscapeSequences` in the xUnit.net command line interface is expressed as `--method-display-options replacePeriodWithComma useEscapeSequences` in the Microsoft Testing Platform command line interface._

There are several switches that are native to Microsoft Testing Platform, and they are discussed here: [https://learn.microsoft.com/dotnet/core/testing/unit-testing-platform-intro#options](https://learn.microsoft.com/dotnet/core/testing/unit-testing-platform-intro#options). Any xUnit.net command line option that isn't listed here, is either covered by one of the Microsoft Testing Platform switches, or is not available in Microsoft Testing Platform command line mode.

We have added one new switch (`--xunit-info`) which allows you to see the output that you'd normally see from the native xUnit.net command line experience, combined with the output from Microsoft Testing Platform:

```
$ dotnet run -- --xunit-info
xUnit.net v3 Microsoft.Testing.Platform Runner v1.0.0+5b41c61aa1 (64-bit .NET 8.0.11)

xUnit.net v3 In-Process Runner v1.0.0+5b41c61aa1 (64-bit .NET 8.0.11)
  Discovering: MyFirstUnitTests
  Discovered:  MyFirstUnitTests
  Starting:    MyFirstUnitTests
    MyFirstUnitTests.UnitTest1.MyFirstTheory(value: 6) [FAIL]
      Assert.True() Failure
      Expected: True
      Actual:   False
      Stack Trace:
        UnitTest1.cs(28,0): at MyFirstUnitTests.UnitTest1.MyFirstTheory(Int32 value)
failed MyFirstUnitTests.UnitTest1.MyFirstTheory(value: 6) (0ms)
  Assert.True() Failure
  Expected: True
  Actual:   False
    at MyFirstUnitTests.UnitTest1.MyFirstTheory(Int32 value) in UnitTest1.cs:28
    MyFirstUnitTests.UnitTest1.FailingTest [FAIL]
      Assert.Equal() Failure: Values differ
      Expected: 5
      Actual:   4
      Stack Trace:
        UnitTest1.cs(14,0): at MyFirstUnitTests.UnitTest1.FailingTest()
failed MyFirstUnitTests.UnitTest1.FailingTest (2ms)
  Assert.Equal() Failure: Values differ
  Expected: 5
  Actual:   4
    at MyFirstUnitTests.UnitTest1.FailingTest() in UnitTest1.cs:14
  Finished:    MyFirstUnitTests
=== TEST EXECUTION SUMMARY ===
   MyFirstUnitTests  Total: 5, Errors: 0, Failed: 2, Skipped: 0, Not Run: 0, Time: 0.070s

Test run summary: Failed! - bin\Debug\net8.0\MyFirstUnitTests.dll (net8.0|x64)
  total: 5
  failed: 2
  succeeded: 3
  skipped: 0
  duration: 142ms
```

### Additional Microsoft Testing Platform features

If you enable the Microsoft Testing Platform command line experience, you will also be able to take advantage of their extension system to add new features to your test project. These include reporting extensions, code coverage extensions, and more. For more information, visit: [https://learn.microsoft.com/dotnet/core/testing/unit-testing-platform-extensions](https://learn.microsoft.com/dotnet/core/testing/unit-testing-platform-extensions).

We have created documentation describing how to get [code coverage with Microsoft Testing Platform](code-coverage-with-mtp), since the standard Coverlet experience is not supported.


## Enabling the `dotnet test` experience

_**Note:** As of the writing of this document, the Microsoft Testing Platform `dotnet test` experience is still experimental._

By default, xUnit.net v3 projects use VSTest when run via `dotnet test`, which comes from the `xunit.runner.visualstudio` package reference.

This is what the VSTest `dotnet test` output looks like (examples using our test project from the [Getting Started with the command line](cmdline) documentation):

```
$ dotnet test
  Determining projects to restore...
  All projects are up-to-date for restore.
  MyFirstUnitTests -> bin\Debug\net8.0\MyFirstUnitTests.dll
Test run for bin\Debug\net8.0\MyFirstUnitTests.dll (.NETCoreApp,Version=v8.0)
VSTest version 17.11.1 (x64)

Starting test execution, please wait...
A total of 1 test files matched the specified pattern.
[xUnit.net 00:00:00.37]     MyFirstUnitTests.UnitTest1.FailingTest [FAIL]
[xUnit.net 00:00:00.37]     MyFirstUnitTests.UnitTest1.MyFirstTheory(value: 6) [FAIL]
  Failed MyFirstUnitTests.UnitTest1.FailingTest [11 ms]
  Error Message:
   Assert.Equal() Failure: Values differ
Expected: 5
Actual:   4
  Stack Trace:
     at MyFirstUnitTests.UnitTest1.FailingTest() in UnitTest1.cs:line 14
  Failed MyFirstUnitTests.UnitTest1.MyFirstTheory(value: 6) [< 1 ms]
  Error Message:
   Assert.True() Failure
Expected: True
Actual:   False
  Stack Trace:
     at MyFirstUnitTests.UnitTest1.MyFirstTheory(Int32 value) in UnitTest1.cs:line 28

Failed!  - Failed:     2, Passed:     3, Skipped:     0, Total:     5, Duration: 33 ms - MyFirstUnitTests.dll (net8.0)
```

To enable the new `dotnet test` experience, add the following property to your project file (.csproj/.fsproj/.vbproj):

```xml
<PropertyGroup>
  <TestingPlatformDotnetTestSupport>true</TestingPlatformDotnetTestSupport>
</PropertyGroup>
```

This is what the Microsoft Testing Platform `dotnet test` output looks like:

```
$ dotnet test
  Determining projects to restore...
  All projects are up-to-date for restore.
  MyFirstUnitTests -> bin\Debug\net8.0\MyFirstUnitTests.dll
  Run tests: 'bin\Debug\net8.0\MyFirstUnitTests.dll' [net8.0|x64]
  Failed! - Failed: 2, Passed: 3, Skipped: 0, Total: 5, Duration: 128ms
bin\Debug\net8.0\MyFirstUnitTests.dll : error run failed: Tests failed: 'bin\Debug\net8.0\TestResults\MyFirstUnitTests_net8.0_x64.log' [net8.0|x64] [MyFirstUnitTests.csproj]
```

A log file is always generated from `dotnet test` runs, but is usually only shown when the test run failed. The failure log in this case looks like:

```
xUnit.net v3 Microsoft.Testing.Platform Runner v1.0.0+5b41c61aa1 (64-bit .NET 8.0.11)

failed MyFirstUnitTests.UnitTest1.MyFirstTheory(value: 6) (0ms)
  Assert.True() Failure
  Expected: True
  Actual:   False
    at MyFirstUnitTests.UnitTest1.MyFirstTheory(Int32 value) in UnitTest1.cs:28
failed MyFirstUnitTests.UnitTest1.FailingTest (2ms)
  Assert.Equal() Failure: Values differ
  Expected: 5
  Actual:   4
    at MyFirstUnitTests.UnitTest1.FailingTest() in UnitTest1.cs:14

Test run summary: Failed! - bin\Debug\net8.0\MyFirstUnitTests.dll (net8.0|x64)
  total: 5
  failed: 2
  succeeded: 3
  skipped: 0
  duration: 169ms

=== COMMAND LINE ===
C:\Program Files\dotnet\dotnet.exe exec bin\Debug\net8.0\MyFirstUnitTests.dll --internal-msbuild-node testingplatform.pipe.f168a78a9c774ad083ef24761b77d00f
```

The same command line options available in the Microsoft Testing Platform command line experience (described in the table above) are also available for `dotnet test`. The command line options are passed after `--`. For example, to filter tests to a single class with the Microsoft Testing Platform `dotnet test` experience, you could run: `dotnet test -- --filter-class ClassName`. This includes command line options from any [Microsoft Testing Platform features](#additional-microsoft-testing-platform-features) you may add. _**Note:** These command line options are available for `dotnet test` regardless of whether you enable the Microsoft Testing Platform command line experience._

You can find additional configuration options for the Microsoft Testing Platform `dotnet test` integration here: [https://learn.microsoft.com/dotnet/core/testing/unit-testing-platform-integration-dotnet-test#additional-msbuild-options](https://learn.microsoft.com/dotnet/core/testing/unit-testing-platform-integration-dotnet-test#additional-msbuild-options)


## Enabling the Test Explorer experience

_**Note:** As of the writing of this document, the Microsoft Testing Platform Test Explorer experience is still experimental, and is only available in preview versions of Visual Studio 2022. The screen shots below were taken with VS2022 17.12 Preview 2._

Like all Microsoft Testing Platform test framework projects, xUnit.net v3 projects are automatically enabled for the new Microsoft Testing Platform Test Explorer experience.

The visual differences from Test Explorer in "VSTest mode" and Test Explorer in "Microsoft Testing Platform mode" with xUnit.net v3 projects are quite subtle, and most of the available functionality is the same. There is one notable improvement for F# projects: the metadata given to Test Explorer in Microsoft Testing Platform mode relating to test namespaces vs. class names allows it to properly understand [test names with periods in them](https://github.com/xunit/xunit/issues/3013).

From a UI perspective, here is what you'll see in VSTest mode:

![Test Explorer (VSTest mode)](/images/getting-started/v3/test-explorer-vstest-mode.png){: .border }

And here is what you'll see in Microsoft Testing Platform mode:

![Test Explorer (Microsoft Testing Platform mode)](/images/getting-started/v3/test-explorer-mtp-mode.png){: .border }

You should be able to simply use Test Explorer in either mode to run and debug your tests. If you experience any issues with Test Explorer in Microsoft Testing Platform mode, you can disable it by adding the following property to your project file (.csproj/.fsproj/.vbproj):

```xml
<PropertyGroup>
  <DisableTestingPlatformServerCapability>true</DisableTestingPlatformServerCapability>
</PropertyGroup>
```

Just remember that VSTest mode requires the package references to `xunit.runner.visualstudio` and `Microsoft.NET.Test.Sdk`. We recommend you keep these package references for backward compatibility, not only with 3rd party runners like Resharper/Rider/CodeRush/VSCode, but also for older versions of Visual Studio that do not support Microsoft Testing Platform.
