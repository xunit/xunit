---
layout: default
title: Migrating from v2 to v3 [Unit test authors]
breadcrumb: Documentation
---

# Migrating from v2 to v3 [Unit test authors]

## As of: 2025 January 9 (`1.0.0`)

This migration guide aims to be a comprehensive list helping developers migrate from xUnit.net v2 to v3. This guide is focused on what to expect for unit test authors. Extensibility authors will want to review this document, and then read the [migration guide specifically for extensibility authors](migration-extensibility).

Because this is a comprehensive guide, you may wish to only skim parts of it, and use search functionality to find information on specific issues that arise, rather than trying to read the guide entirely. You should read the first informational section titled "Architectural Changes", then follow the next three sections related to (a) updating NuGet packages, (b) updating to create an executable instead of a library, and (c) updating your target framework. All sections after that should be consider reference material.

In addition to this migration document (which only covers the differences between v2 and v3), we have a parallel document which covers [what's new in v3](whats-new). The "What's New" document describes newly available features (including information on the best way to create new xUnit.net v3 projects), and should be consulted after you've successfully migrated your project from v2 to v3.

The current builds are:

{: .table .latest }
Package                     | NuGet Version                                                                                                                               | [CI Version](/docs/using-ci-builds)
--------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------
`xunit.v3.*`                | [![](https://img.shields.io/nuget/vpre/xunit.v3.svg?logo=nuget)](https://www.nuget.org/packages/xunit.v3)                                   | [![](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fxunit%2Fxunit%2Fshield%2Fxunit.v3%2Flatest&color=f58142)](https://feedz.io/org/xunit/repository/xunit/packages/xunit.v3)
`xunit.analyzers`           | [![](https://img.shields.io/nuget/vpre/xunit.analyzers.svg?logo=nuget)](https://www.nuget.org/packages/xunit.analyzers)                     | [![](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fxunit%2Fxunit%2Fshield%2Fxunit.analyzers%2Flatest&color=f58142)](https://feedz.io/org/xunit/repository/xunit/packages/xunit.analyzers)
`xunit.runner.visualstudio` | [![](https://img.shields.io/nuget/vpre/xunit.runner.visualstudio.svg?logo=nuget)](https://www.nuget.org/packages/xunit.runner.visualstudio) | [![](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fxunit%2Fxunit%2Fshield%2Fxunit.runner.visualstudio%2Flatest&color=f58142)](https://feedz.io/org/xunit/repository/xunit/packages/xunit.runner.visualstudio)

Note that while we attempt to ensure that CI builds are always usable, we cannot make guarantees. If you come across issues using a CI build, please [let us know](https://github.com/xunit/xunit/issues)!

## Table of Contents

* [Architectural Changes](#architectural-changes)
* [Migrating to v3 Packages](#migrating-to-v3-packages)
* [Convert to Executable Project](#convert-to-executable-project)
* [Update Target Framework](#update-target-framework)
* [Removal of `xunit.abstractions`](#removal-of-xunitabstractions)
* [Changes to Assertion Library](#changes-to-assertion-library)
* [Changes to Core Framework](#changes-to-core-framework)
* [Changes to Runner Utility](#changes-to-runner-utility)


## Architectural Changes

Before we talk about the migration process, we need to provide information about architectural changes that have occurred between v2 and v3 that impact you as a developer.

### New minimum runtime requirements

We have set new minimum runtime requirements for xUnit.net v3:

* .NET Framework 4.7.2 (or later)
* .NET 6 (or later)

Our target frameworks for v3 currently are:

* .NET Standard 2.0 (`netstandard2.0`)
* .NET Framework 4.7.2 (`net472`)
* .NET 6 (`net6.0`)

Also new for v3: Mono is officially supported on Linux and macOS for .NET Framework test projects. While it did often work with v1 and v2, we do officially test and verify with it now.

For a complete list of which packages target which frameworks, please see [this issue](https://github.com/xunit/xunit/issues/2330).

### Stand-alone executables

Test projects in v2 are library projects, and runners were required to run the tests. Those runners loaded the assemblies into their own address space and ran them from there. This was a design from back in .NET Framework days, where Application Domains could be used as a semi-effective isolation layer between the runner and the unit test code. This relatively complex isolation system was removed from .NET Core, so it's no longer available. Application Domains not only solved a very real isolation problem (and loading/unloading problem), but also a much more significant dependency resolution problem.

Test projects in v3 are stand-alone executables now, capable of running themselves. Rather than worrying about dependency resolution at runtime, we allow the compiler to do its job for us. The isolation of running your tests in a separate process is significantly simpler and more effective than Application Domains were in .NET Framework, and works for .NET as well.

When you build a v3 test project, the result is directly executable.

* For .NET Framework projects, the resulting `.exe` file is your unit test project and can be run
* For .NET projects, the resulting `.dll` file is your unit test project, and the build process also creates a `.exe` file (or extension-less executable, on Linux and macOS) that can be used to run your test project. _(It's important to remember that the executable for .NET projects is just a stub launcher; the actual unit tests live inside the `.dll`, and when using any multi-assembly runner, you will pass the path to the `.dll` file for .NET projects, not the `.exe` file.)_

If you run the executable without any command line options, all your tests will run:

{: .border .shrink-75 }
![](/images/v3-migration/stand-alone-executable.png)

You can pass `-?` to the executable for a complete list of command line switches that are available. The list will be very similar to the console runner's command line options, except slightly reduced because of the fact that you're only running a single test assembly.

If you want to build and run in a single step, `dotnet run` will work with both .NET Framework and .NET test projects. Just bear in mind that passing command line options with `dotnet run` requires prefixing options for the test project with `--`. For example, if you want to generate an XML report of the test run, these two are equivalent:

```
$ dotnet build
$ .\bin\Debug\TestProject.exe -xml results.xml
```

```
$ dotnet run -- -xml results.xml
```

### Only SDK-style projects are supported

While we are aware that you may be able to make xUnit.net v3 work with older, pre-SDK-style projects, this is not a supported scenario by our team.

### async void tests are no longer supported

Tests which are `async void` will be fast-failed at runtime to indicate that their signatures need to be updated from `void` to either `Task` or `ValueTask`.

### `IAsyncLifetime` now inherits from `IAsyncDisposable` and disposal guidelines have been updated

In v2, `IAsyncLifetime` defined its own `DisposeAsync` method, and if you implemented both `IAsyncLifetime` and `IDisposable`, we would call both `DisposeAsync` and `Dispose`.

In v3, `IAsyncLifetime` now inherits `IAsyncDisposable`, so the `DisposeAsync` method comes from there. We are also now following framework guidance which says that when an object implements both `IAsyncDisposable` and `IDisposable`, you should only call one or the other, and not both. For xUnit.net, that means it will call `DisposeAsync` but not `Dispose`. This is true even for objects which implement both `IAsyncDisposable` and `IDisposable`, regardless of whether they implement `IAsyncLifetime` or not.

This could be a breaking change if you were previously relying on us calling both.

### Attribute instance lifetime may differ

Due to differences in the way v2 and v3 acquire attribute instances, it may appear that we are now caching attribute instances where we previously did not. The truth is that the previous behavior (of over-creating attribute instances) was actually the bug in this scenario, as it differs from the normal .NET behavior (where attribute instances are cached when they're first created).


## Migrating to v3 Packages

Most of the packages for v3 Core Framework have moved to new names that start with `xunit.v3`.

Change the following package references (and use versions from the table at the top of this page):

{: .table .latest }
v2 package                                                      | v3 package
--------------------------------------------------------------- | ----------
`xunit`                                                         | `xunit.v3`
`xunit.abstractions`                                            | _Remove, no longer required_
`xunit.analyzers`                                               | _Unchanged_
`xunit.assert`                                                  | `xunit.v3.assert`
`xunit.assert.source`                                           | `xunit.v3.assert.source`
`xunit.console`                                                 | _Remove, no longer supported_
`xunit.core`                                                    | `xunit.v3.core`
`xunit.extensibility.core`<br />`xunit.extensibility.execution` | `xunit.v3.extensibility.core` (*)
`xunit.runner.console`                                          | `xunit.v3.runner.console`
`xunit.runner.msbuild`                                          | `xunit.v3.runner.msbuild`
`xunit.runner.reporters`<br />`xunit.runner.utility`            | `xunit.v3.runner.utility` (*)
`xunit.runner.visualstudio`                                     | _Make sure to pick up a 3.x.y version_

_**Note:** In some cases multiple libraries/packages were merged together into a single new library/package, as denoted in the table above with (*)._

If you want to use the MSBuild runner, we now ship both a .NET Framework and .NET Core/.NET version in the same package. It will dynamically select the correct version depending on whether you use the .NET Framework MSBuild or the .NET MSBuild (via `dotnet build` or `dotnet msbuild`). However, the .NET version only supports v3 test projects. If you need to still run v1 and/or v2 test projects, you must use the .NET Framework version. (Mono ships with a .NET Framework version of MSBuild, so all comments about .NET Framework also apply to Mono.)

### Why did we change the package names?

We changed the package naming scheme from `xunit.*` to `xunit.v3.*` for two primary reasons and one secondary reason:

* We wanted users to make a conscious choice to upgrade and understand what the scope of that work is, rather than being offered a NuGet package upgrade in Visual Studio and then have everything be broken without being told why.
* We have frequently been asked to observe SemVer for our package versions, which has been impossible previously. Our package naming and versioning system predates SemVer, and trying to adopt it after the fact would be painful. The `2` in the `2.x.y` package versioning scheme implied a _**product version**_ but it was living in the major version of the package. The new package name allows the v3 _**product version**_ to live in the package name instead of the major version, and this allows us to evolve those package versions according to SemVer without implying a new production version has been released.

The secondary reason was:

* As shown above, some packages have been merged (and new intermediate packages have been introduced). We previously tried the "upgrade an obsoleted package" strategy from v1 -> v2 with the `xunit.extensions` package and found that process less than ideal for most users. This is not an area where NuGet is particularly helpful. We would've preferred that we could have automatically removed `xunit.extensions` rather than having a v2 version in place with no code inside as a dead reference. By having users follow this migration guide, we can clearly tell them which packages changed and which should be removed.


## Convert to Executable Project

Update your project file (i.e., `.csproj`) and change `OutputType` from `Library` to `Exe`. You may need to add `OutputType` if it's not present, since `Library` is the default value:

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
</PropertyGroup>
```


## Update Target Framework

Per the new [minimum target framework versions](#new-minimum-runtime-requirements), make sure to update your target framework(s) if you're currently targeting something that's no longer supported.

At this point, you should be able to successfully run `dotnet restore`.

From this point forward, we will discuss the changes you'll see, and where common compilation errors might occur.


## Removal of `xunit.abstractions`

The `xunit.abstractions` package in v2 was used to communicate across the Application Domain between unit tests and unit test runners. Now that the runner lives in the same process with your unit test project, this abstraction layer is no longer necessary.

There are several abstraction interfaces that were previously in the `Xunit.Abstractions` namespace that have been moved to new namespaces. Because `xunit.v3.runner.utility` still needs to link against `xunit.abstractions` to be able to run v2 tests, these types all needed to move into new namespaces to prevent naming collisions with the v2 types.

The follow types have been moved to `Xunit` (in `xunit.v3.extensibility.core`):

* `ITestOutputHelper`

The following types have been moved to `Xunit.Runner.Common` (in `xunit.v3.runner.common`):

* `ISourceInformationProvider`

The following types have been moved to `Xunit.Sdk` (in `xunit.v3.common`):

* `IAfterTestFinished`
* `IAfterTestStarting`
* `IBeforeTestFinished`
* `IBeforeTestStarting`
* `IDiagnosticMessage` (note that internal diagnostic messages now send `IInternalDiagnosticMessage`)
* `IDiscoveryCompleteMessage` (renamed to `IDiscoveryComplete`)
* `IErrorMessage`
* `IFailureInformation` (renamed to `IErrorMetadata`)
* `IFinishedMessage` (renamed to `IExecutionSummaryMetadata`)
* `IMessageSink`
* `IMessageSinkMessage`
* `ITest`
* `ITestAssembly`
* `ITestAssemblyCleanupFailure`
* `ITestAssemblyFinished`
* `ITestAssemblyMessage`
* `ITestAssemblyStarting`
* `ITestCase`
* `ITestCaseCleanupFailure`
* `ITestCaseDiscoveryMessage` (renamed to `ITestCaseDiscovered`)
* `ITestCaseFinished`
* `ITestCaseMessage`
* `ITestCaseStarting`
* `ITestClass`
* `ITestClassCleanupFailure`
* `ITestClassConstructionFinished`
* `ITestClassConstructionStarting`
* `ITestClassDisposeFinished`
* `ITestClassDisposeStarting`
* `ITestClassFinished`
* `ITestClassMessage`
* `ITestClassStarting`
* `ITestCleanupFailure`
* `ITestCollection`
* `ITestCollectionCleanupFailure`
* `ITestCollectionFinished`
* `ITestCollectionMessage`
* `ITestCollectionStarting`
* `ITestFailed`
* `ITestFinished`
* `ITestFrameworkDiscoveryOptions`
* `ITestFrameworkExecutionOptions`
* `ITestFrameworkOptions`
* `ITestMessage`
* `ITestMethod`
* `ITestMethodCleanupFailure`
* `ITestMethodFinished`
* `ITestMethodMessage`
* `ITestMethodStarting`
* `ITestOutput`
* `ITestPassed`
* `ITestResultMessage`
* `ITestSkipped`
* `ITestStarting`
* `IXunitSerializable`
* `IXunitSerializationInfo`

The following types have been moved to `Xunit.v3` (in `xunit.v3.extensibility.core`):

* `ITestFramework`
* `ITestFrameworkDiscoverer`
* `ITestFrameworkExecutor`

The following types have been removed:

* `IAssemblyInfo`
* `IAttributeInfo`
* `IExecutionMessage`
* `IMethodInfo`
* `IParameterInfo`
* `IReflectionAssemblyInfo`
* `IReflectionAttributeInfo`
* `IReflectionMethodInfo`
* `IReflectionParameterInfo`
* `IReflectionTypeInfo`
* `ISourceInformation` (replaced with `SourceInformation`)
* `ITypeInfo`


## Changes to Assertion Library

By and large, the assertion library in v3 is a small superset of the assertion library in v2 2.9. There are new assertions as well as overloads to existing assertions that might conflict with any custom assertions you've added and/or might cause compilation issues due to ambiguous overloads now available.

A complete list of what was added is available in the [what's new in v3](whats-new#whats-new-in-the-assertion-library) document.


## Changes to Core Framework

The core framework has undergone some extension re-working internally. We aimed to make as few disruptive changes as possible, trying to limit those to where it was unavoidable and/or the usability improvement warranted the change. We hope that the vast majority of these will involve just modifying or cleaning up `using` statements.

### Attributes that took type name and assembly name strings

There were several attributes in the system that allowed you to take type names as two strings: the fully qualified type name, and the name of the assembly where that type resides. This was to support source-based test discovery, which is a feature that has been removed from v3.

These attributes have been updated so that their constructors now simply take a `Type`, and you can use `typeof` to specify the type of the object.

For example:

```csharp
[assembly: CollectionBehavior("MyNamespace.MyCollectionFactory", "MyAssembly")]
```

can be converted to:

```csharp
[assembly: CollectionBehavior(typeof(MyCollectionFactory))]
```

The list of the affected attributes include:

* `CollectionBehaviorAttribute`
* `TestCaseOrdererAttribute`
* `TestCollectionOrdererAttribute`
* `TestFrameworkAttribute`

### Removal of reflection abstractions

The removal of reflection abstractions means that many previous splits between attributes and discoverers have been collapsed, and the attributes become responsible for their own behavior rather than relying on discoverers (which were previously written in terms of the reflection abstractions rather than depending on compiled code).

One obvious example of this is the removal of `IDataDiscoverer` (previously in `Xunit.Abstractions`) and `DataDiscoverer` (previously in `Xunit.Sdk`). The `IDataDiscoverer.GetData` method has been moved to `IDataAttribute.GetData`, and becomes an abstract method on the base `DataAttribute` class. The previous method was given an `IAttributeInfo` (which pointed to the `DataAttribute`-derived attribute) and `IMethodInfo` (which pointed to the test method); the updated method provides access to the `MethodInfo` (rather than the reflection abstraction version), and also to a disposal tracker so that it can add any data that it creates which might need to be disposed when cleaning up.

If you're looking for a type that has disappeared and it's a discoverer, chances are the thing it previous discovered is now responsible for describing itself, rather than relying on an external discoverer.

### Namespace changes

Many of the namespace changes here have been done in the name of consistency. Generally speaking, when you see a type that now lives in the `Xunit.Sdk` namespace, it comes from `xunit.v3.common`, and when you see it in the `Xunit.v3` namespace, it comes from `xunit.v3.extensibility.core`.

* The concrete test messages in the `Xunit.Sdk` namespace (for example, `DiagnosticMessage`) have moved to the `Xunit.v3` namespace. In the case where messages had non-empty constructors (for example, `ErrorMessage`) they have been converted to a single parameterless constructor (for serialization/deserialization purposes), and helper static creation methods like `FromException` have been added to replace the previous constructor usage.

* The runner classes (i.e., `TestAssemblyRunner`, `TestCollectionRunner`, etc.) have moved from `Xunit.Sdk` to `Xunit.v3`. The non-abstract classes (i.e., `XunitTestAssemblyRunner`, `XunitTestCollectionRunner`, etc.) have been converted to singletons. As such, their constructors have been replaced by `RunAsync` methods which accept the information previously passed to the constructor. Their state is now stored in context classes that are constructed and passed between layers. The base runner classes now also have expanded generic types required to match the new context requirements.

* `CollectionPerAssemblyTestCollectionFactory` and `CollectionPerTestClassTestCollectionFactory` have moved from the `Xunit.Sdk` to the `Xunit.v3` namespace.

* The abstract `DataAttribute` class has moved from the `Xunit.Sdk` to the `Xunit.v3` namespace.

* `DiscoveryCompleteMessage` has been renamed to `DiscoveryComplete`, and moved from `Xunit.Sdk` to `Xunit.v3`.

* `DisplayNameFormatter` has moved from `Xunit.Sdk` to `Xunit.v3`.

* `ExceptionAggregator` has moved from `Xunit.Sdk` to `Xunit.v3`, and been converted to a `struct` rather than a `class`. The `RunAsync` methods have been converted from `Task` to `ValueTask` to align with the framework moving to `ValueTask` internally.

* `ExecutionErrorTestCase` has moved from `Xunit.Sdk` to `Xunit.v3`, and its primary constructor has been updated.

* `ExecutionTimer` has moved from `Xunit.Sdk` to `Xunit.v3`, made static, and its `Aggregate` methods have been renamed to `Measure`. The async versions have converted from accepting `Task` to accepting `ValueTask` and `ValueTask<T>`. These methods now return the measured time rather than aggregating a count inside the timer itself.

* `ExtensibilityPointFactory` has moved from `Xunit.Sdk` to `Xunit.v3` and been completely overhauled, given the removal of the reflection abstractions when the source-based discovery feature was removed from v3.

* `ITestCaseOrderer` has moved from `Xunit.Sdk` to `Xunit.v3`, and `OrderTestCases` now both accepts and returns `IReadOnlyCollection<TTestCase>` instead of `IEnumerable<TTestCase>`.

* `ITestCollectionOrderer` has moved from `Xunit` to `Xunit.v3`, and `OrderTestCollections` now both accepts and returns `IReadOnlyCollection<TTestCollection>` instead of `IEnumerable<TTestCollection>`.

* `IXunitTestCaseDiscoverer` has moved from `Xunit.Sdk` to `Xunit.v3` and the `Discover` method has been made async.

* `IXunitTestCollectionFactory` has moved from `Xunit.Sdk` to `Xunit.v3`.

* `MaxConcurrencySyncContext` has moved from `Xunit.Sdk` to `Xunit.v3`.

* `TestCollectionComparer` has moved from `Xunit.Sdk` to `Xunit.v3` and been made generic.

### Types removed

* The `AsyncTestSyncContext` class which made `async void` tests work has been removed.

* `AssemblyTraitAttribute` has been removed and `TraitAttribute` can be used on assemblies now.

* `LongLivedMarshalByRefObject` (in the `Xunit` namespace) has been removed from the core framework, since v3 does not support running in Application Domains.

* `PlatformSpecificAssemblyAttribute` has been removed, as `xunit.v3.extensibility.core` is based purely on `netstandard2.0` now rather than the previous multi-targeted `xunit.execution.*` libraries.

* `PropertyDataAttribute`, which had been obsoleted in favor of `MemberDataAttribute`, has been removed.

* `Reflector` (and all the `ReflectionXyz` classes it returned) has been removed, since the reflection abstractions have been removed from v3.

* `TestCaseBulkDeserializer` and `TestCaseDescriptorFactory` have been removed, as the cross-Application Domain performance problems they were solving are no longer present in v3.

* `TestClassException` has been removed, and replaced with a more generalized `TestPipelineException`.

### Miscellaneous changes

* In an effort to track down and eliminate cases of double enumeration, many method signatures may have changed from `IEnumerable<T>` to `IReadOnlyCollection<T>` when it's known that the data is safe to double enumerate.

* `BeforeAfterTestAttribute` is now given the instance of `IXunitTest` for the currently running test.

* `DisposalTracker` has been switched from `IDisposable` to `IAsyncDisposable`, and is capable of disposing of objects which implement either interface. Note that if an object implements both interfaces, only `IAsyncDisposable` will be called.

* `ExceptionUtility.ConvertExceptionToFailureInformation` has been replaced by `.ExtractMetadata`, which returns the exception metadata as a tuple rather than an interface.

* `IAsyncLifetime` has been updated to inherit from `IAsyncDisposable`, and both `DisposeAsync` and `InitializeAsync` have been converted from `Task` to `ValueTask`.

* `MemberDataAttributeBase.Parameters` has been renamed to `Arguments` to better align with the fact that it contains parameter argument values, not parameters. `ConvertDataItem` has been renamed to `ConvertDataRow` (and now returns the new `ITheoryDataRow` object rather than `object[]`). The `GetDataMethod` has been made async, and now returns `IReadOnlyCollection<ITheoryDataRow>` rather than `IEnumerable<object[]>`.

* `Record.ExceptionAsync` has been updated to return `ValueTask` as well as accepting lambdas that return `ValueTask`.

* `SerializationHelper.GetType` and `.GetTypeNameForSerialization` have been replaced with `.SerializedTypeNameToType` and `.TypeToSerializedTypeName`, respectively. A non-generic version of `.Deserialize` has been added, and the `.Serialize` method now requires a `Type` to be passed in addition to the value.

* `TheoryDataBase` is the new base type for the `TheoryData<>` classes as well as the untyped `TheoryData`.


## Changes to Runner Utility

The runner utility libraries have had a fairly extensive overhaul. The two previous libraries (`xunit.runner.reporters` and `xunit.runner.utility`) were merged into a single library (`xunit.v3.runner.utility`). Most of the types in this library have retained the `Xunit` namespace, just like they previously had. Some sub-namespaces have been introduced to isolate code that's specific to running v1 vs. v2 vs. v3 tests.

A second new library (`xunit.v3.runner.common`) was introduced. Types in this library primarily have the `Xunit.Runner.Common` namespace.

The split between these two libraries comes from the fact that we now have an in-process console runner (that is the runner that is linked into your unit test projects, from the `xunit.v3.runner.inproc.console` package) and an out-of-process console runner (the one in `xunit.v3.runner.console`). The former only has to be able to run v3 test projects, whereas the latter has to be able to run projects from v1, v2, and v3.

The in-process console runner takes dependencies on `xunit.v3.extensibility.core` and `xunit.v3.runner.common` to be able to perform its runner duties, whereas `xunit.v3.runner.utility` takes dependencies only on `xunit.abstractions` to be able run v2 tests (and `Mono.Cecil` to be able to read assembly metadata without loading the assembly into memory).

At the moment, third party reporters are not supported. We have an [open issue](https://github.com/xunit/xunit/issues/1874) to solve the problem of how to enable third party reporters without creating the strong dependency on `xunit.v3.runner.utility`, as the strong dependency in v2 on `xunit.runner.utility` made writing third party reporters exceptionally fragile.

### Updated support for custom runner reporters

We have overhauled the way custom runner reporters are supported in v3. The new design for runner reporters directly links them into your test assembly, and the reporter is now chosen via the `-reporter` switch (only available when you run your test project directly).

For more information, see the [documentation page](/docs/getting-started/v3/custom-runner-reporter.md).

### Namespace changes

* The concrete test messages in the `Xunit` namespace (for example, `DiagnosticMessage`) have moved to the `Xunit.Runner.Common` namespace. In the case where messages had non-empty constructors (for example, `ErrorMessage`) they have been converted to a single parameterless constructor (for serialization/deserialization purposes), and helper static creation methods like `FromException` have been added to replace the previous constructor usage.

* The runner reporters (i.e., `AppVeyorReporter`, `DefaultRunnerReporter`, etc.) and their message handlers have moved from `Xunit.Runner.Reporters` to `Xunit.Runner.Common`.

* `AggregateMessageSink` has moved from `Xunit` to `Xunit.Runner.Common`.

* `AppDomainSupport` has moved from `Xunit` to `Xunit.Runner.Common`.

* `ConfigReader`, `ConfigReader_Configuration`, and `ConfigReader_Json` have moved from `Xunit` to `Xunit.Runner.Common`. The `Load` method overloads that took `Stream` have been removed.

* `ConsoleRunnerLogger` has moved from `Xunit` to `Xunit.Runner.Common`, and has been consolidated down to a single constructor that requires passing a `ConsoleHelper` object.

* `DefaultTestCaseBulkDeserializer` and `ITestCaseBulkDeserializer` have moved from `Xunit` to `Xunit.Internal` and are no longer directly supported for third party usage.

* `DiagnosticEventSink` has moved from `Xunit` to `Xunit.Runner.Common`.

* `DiscoveryCompleteMessage` has been renamed to `DiscoveryComplete`, and moved from `Xunit` to `Xunit.Runner.Common`.

* `DiscoveryEventSink` has moved from `Xunit` to `Xunit.Runner.Common`.

* `ExecutionEventSink` has moved from `Xunit` to `Xunit.Runner.Common`.

* `ExecutionSink` has moved from `Xunit` to `Xunit.Runner.Common`.

* `ExecutionSinkOptions` has moved from `Xunit` to `Xunit.Runner.Common`.

* `ExecutionSummary` has moved from `Xunit` to `Xunit.Runner.Common`.

* `IMessageSinkWithTypes` and `IMessageSinkMessageWithTypes` have moved from `Xunit` to `Xunit.Runner.v2`.

* `IRunnerLogger` has moved from `Xunit` to `Xunit.Runner.Common`.

* `IRunnerReporter` has moved from `Xunit` to `Xunit.Runner.Common`.

* `IXunit1Executor` has moved from `Xunit` to `Xunit.Runner.v1`.

* `LongRunningTestsSummary` has moved from `Xunit` to `Xunit.Runner.Common`.

* `MessageHandler<T>`, `MessageHandlerArgs`, and `MessageHandlerArgs<T>` have moved from `Xunit` to `Xunit.Runner.Common`.

* `NullSourceInformationProvider` has moved from `Xunit` to `Xunit.Runner.Common`.

* `RunnerEventSink` has moved from `Xunit` to `Xunit.Runner.Common`.

* `RunnerReporterUtility` has moved from `Xunit` to `Xunit.Runner.Common`, and now offers a way to directly retrieve just the embedded runner reporters.

* `StackFrameInfo` and `StackFrameTransformer` have moved from `Xunit` to `Xunit.Runner.Common`.

* `TestCaseDiscoveryMessage` has been renamed to `TestCaseDiscovered`, and has moved from `Xunit` to `Xunit.Runner.Common`.

* `TestFrameworkOptions` has moved from `Xunit` to `Xunit.Runner.Common` and now can be serialized.

* `TransformFactory` has moved from `Xunit.ConsoleClient` to `Xunit.Runner.Common`.

* `Xunit1`, `Xunit1Executor`, `Xunit1RunSummary`, and `Xunit1TestCase` have moved from `Xunit` to `Xunit.Runner.v1` and been updated for the new front controller contract.

* `Xunit2` has moved from `Xunit` to `Xunit.Runner.v2` and been updated for the new front controller contract.

* `XunitFilters` has moved from `Xunit` to `Xunit.Runner.Common`.

* `XunitProject` and `XunitProjectAssembly` have moved from `Xunit` to `Xunit.Runner.Common` and been significantly restructured.

### Types removed

* `DefaultTestCaseDescriptorProvider` has been removed.

* `DelegatingExecutionSummarySink`, `DelegatingFailSkipSink`, `DelegatingLongRunningTestDetectionSink`, `DelegatingXmlCreationSink`, `FailSkipVisitor`, `XmlAggregateVisitor`, and `XmlTestExecutionVisitor` have been removed. They have been consolidated into a single `ExecutionSink` class that ensures correct ordering behavior. Accordingly, `IExecutionSink` and `IExecutionVisitor` have also been removed.

* `ITestCaseDescriptorProvider` and `TestCaseDescriptor` have been removed.

* `MessageSinkAdapter` and `MessageSinkWithTypesAdapter` have been removed.

* `TestDiscoveryVisitor` has been removed.

* `TestMessageVisitor` and `TestMessageVisitor<T>` have been removed.

### Miscellaneous changes

* `AppVeyorClient` and `VstsClient` have been moved from `public` to `internal`.

* `IFrontController` has had `CanUseAppDomains` removed, and has added `FindAndRun` and `Run` methods to run tests. It has also been converted from `IDisposable` to `IAsyncDisposable`.

* `IFrontControllerDiscoverer` has been added, with a `Find` method for finding tests (without running them), as well as exposing the metadata about the test project (including the `CanUseAppDomains` property that was removed from `IFrontController`).

* `ISourceInformationProvider.GetSourceInformation` has changed its return value from `ISourceInformation` to `(string? sourceFile, int? sourceLine)`.

* `TestExecutionSummary` (for a single test assembly) has been replaced by `TestExecutionSummaries` (for multiple test assemblies).

* `XunitFrontController` has been updated for the new front controller contract, and is now `IAsyncDisposable` instead of `IDisposable`. Bulk deserializer and test descriptor functionality has been removed.
