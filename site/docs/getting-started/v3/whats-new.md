---
layout: default
title: What's New in v3
breadcrumb: Documentation
---

# What's New in v3

## As of: 2024 October 27 (`0.5.0-pre.27`)

This guide aims to be a comprehensive list of the new features added to v3, written for existing developers who are using v2.

In addition to this new features document (which only covers new features in v3), we have a parallel document which covers [migrating from v2 to v3](migration). The "Migrating from v2 to v3" document describes changes that developers will need to understand when porting their v2 projects to v3, as well as when starting new v3 projects armed with their v2 experience.


## Table of Contents

* [New Project Templates](#new-project-templates)
* [What's New in the Assertion Library](#whats-new-in-the-assertion-library)
* [What's New in the Core Framework](#whats-new-in-the-core-framework)
* [What's New in Runner Utility](#whats-new-in-runner-utility)


## New Project Templates

We are shipping new templates for use with `dotnet new` to make it easier to create xUnit.net v3 projects.

### Installing the v3 templates

To install the templates, run the following command:

```
$ dotnet new install xunit.v3.templates
```

We will update the templates with each new release of xUnit.net v3 to NuGet, and you can ensure you're using the latest version of them (and any others that you've installed) with a simple command:

```
$ dotnet new update
```

### Creating a new unit test project

To create a new xUnit.net v3 test project in the current folder, run the following command:

```
$ dotnet new xunit3
```

We ship the templates with three languages in-box: C# (the default), F#, and VB.

Like the in-box v2 templates, the new v3 templates include a reference to `xunit.runner.visualstudio` so that you can immediately use `dotnet test` and Visual Studio's Test Explorer. We also include an empty `xunit.runner.json` (pre-populated with the correct JSON schema) that is correctly set up in your project file.

The template includes a single sample unit test.

### Creating a new extension project

To create a new xUnit.net v3 extension project, run the following command:

```
$ dotnet new xunit3-extension
```

We ship the templates with three languages in-box: C# (the default), F#, and VB.

An extension project is one that aims to create an extensibility point for xUnit.net v3. Unlike a unit test project, these projects only have access to the core framework by default, and in a way that is intended for extensibility.

The template includes a sample extension.


## What's New in the Assertion Library

Items here are related to the assertion library, from the `xunit.v3.assert` and `xunit.v3.assert.source` NuGet packages.

### New assertions for everybody

Three new assertions have been added to support dynamically skipping a test at runtime.

* `Assert.Skip(string message)`
* `Assert.SkipUnless(bool condition, string reason)` will dynamically skip the test only if `condition` is `false`
* `Assert.SkipWhen(bool condition, string reason)` will dynamically skip the test only if `condition` is `true`

### New assertions for projects previously targeting .NET Framework/.NET 5 or older

#### Support for immutable collections

> There are new overloads for `ImmutableDictionary<TKey, TValue>`, `ImmutableHashSet<T>`, `ImmutableSortedSet<T>`, which include:
>
> * `Assert.Contains`
> * `Assert.DoesNotContain`

#### Support for partial collections

> There are new overloads for `Span<T>`, `ReadOnlySpan<T>`, `Memory<T>`, and `ReadOnlyMemory<T>`, which include:
>
> * `Assert.Contains`
> * `Assert.DoesNotContain`
> * `Assert.Equal`
>
> There are new overloads for `Span<char>`, `ReadOnlySpan<char>`, `Memory<char>`, and `ReadOnlyMemory<char>` where those types are treated identically to strings:
>
> * `Assert.Contains`
> * `Assert.DoesNotContain`
> * `Assert.EndsWith`
> * `Assert.Equal`
> * `Assert.StartsWith`

### Updated assertions

* `Assert.Equivalent` now supports comparing two `Uri` values. It does so by comparing that the two `Uri`s have identical `OriginalString` values.


## What's New in the Core Framework

Items here are related to the core framework used for writing tests, from the `xunit.v3.common` and `xunit.v3.core` NuGet packages.

### Dynamically skippable tests

We have added the ability to dynamically skip tests via the `[Fact]` and `[Theory]` attributes, in addition to the `Assert.Skip` family of assertions mentioned above.

The ability to dynamically skip tests has been added to support determining when to skip a test at runtime rather than at compile time. An example of a reason to dynamically skip a test at runtime would include: skipping a test based on the execution environment. That might include the operating system (Windows vs. macOS vs. Linux) or the runtime environment (interactive tests run by developers vs. tests run during continuous integration). The project template `xunit3-extension` illustrates one way to add a dynamic skip system (via an attribute) based on the operating system.

You may set either `SkipUnless` or `SkipWhen` (but not both) to point at a public static property on the test class which returns `bool`; to skip the test, return `false` for `SkipUnless` or `true` for `SkipWhen`. You may also place the public static property on another class by setting `SkipType`. Note that setting both `SkipUnless` and `SkipWhen` will result in a runtime test failure.

During discovery, tests which have `Skip` set along with one of `SkipUnless` or `SkipWhen` will not show up as statically skipped tests; instead, the property value will be inspected at runtime during test execution to determine if it should be skipped at runtime. This means that, unlike statically skipped tests, the entire test pipeline is run for such tests, including `BeforeAfterTestAttribute` instances, test class creation, and test class disposal.

### Explicit tests

We have added support for explicit tests; that is: tests which are normally not run unless it is requested that all explicit tests run (such as a runner command line switch) or through a gesture that indicates that the user wants to run a specific explicit test (such as asking to run it inside Test Explorer).

A test is marked as explicit through the new `Explicit` property on the `[Fact]` and `[Theory]` attributes. Tests which aren't run (because they don't match the requested explicit option) are reported in runners as "not run".

### Test filtering expressions

In addition to supporting our standard command line "simple" filters (like `-class` or `-method`), for v3 runners we have added a new "query filter language" that can be invoked via `-filter`. This query filter language allows more complex filtering operations than were previously possible with the simple filters. These complex filters also work via `xunit.v3.runner.console` when running older tests linked against v1 or v2, as the filtering is performed inside the runner.

For more information, see the [documentation page](/docs/query-filter-language).

### Test context

We have added a `TestContext` class which is designed to:

* Provide (via several properties) information about the current state of the test pipeline.
* Provide (via `.CancellationToken`) a cancellation token that can be passed to downstream methods in your test to help cancellation occur sooner, as well as (via `.CancelCurrentTest`) to attempt to cancel the execution of the current test.
* Provide (via `.SendDiagnosticMessage`) the ability to send diagnostic messages from anywhere.
* Store and retrieve (via `.KeyValueStorage`) information that can be used to communicate between different stages of the test pipeline
* Add (via `.AddAttachment`) attachments to the test results, that will be recorded into the various report formats. Attachments are named, and may contain either plain-text string content or binary content (with an associated MIME media type).
* Add (via `.AddWarning`) warnings to test results, which will be reported to the test runner as well as recorded into the various report formats

Access to the test context is available in two ways:

* You can access it from anywhere via the static `TestContext.Current`.
* You can access it via `ITestContextAccessor`, which can be injected into a test class's constructor alongside fixtures.

Regardless of whether you use `TestContext.Current` or `ITestContextAccessor.Current`, this provides a "moment in time" snapshot of the current state, so the context should be used immediately rather than stored away for later use.

### Theory data rows and metadata

In v2, the contract for theory data providers was that they must return something that was compatible with `IEnumerable<object[]>`.

In v3, this contract has been expanded to allow three different legal data row representations:

* `object[]`
* named or unnamed `Tuple<>`
* `ITheoryDataRow`

In addition, methods from `MemberData` may be async, returning `Task<>` or `ValueTask<>` versions of these, so that data retrieval can be async.

The last type (`ITheoryDataRow`) is new, and represents the ability to decorate each row of data with metadata. Theory data rows may now be:

* Marked as explicit
* Marked as skipped
* Have a custom display name
* Have a timeout
* Have traits

We have introduced a new base class `TheoryDataRow` for untyped data rows, and 1-10 type argument generic versions of `TheoryDataRow<>` for strongly typed data rows. You can use a property-setting pattern for these theory data rows, like:

```csharp
new TheoryDataRow<int, string>(42, "Hello world") { Skip = "Don't run this yet" }
```

as well as a fluent construction pattern:

```csharp
new TheoryDataRow<int, string>(42, "Hello world").WithSkip("Don't run this yet")
```

The analyzers have been updated to flag incorrect type matching with `IEnumerable<TheoryDataRow<...>>` in the same way that we flag incorrect type matching with `TheoryData<...>`.

### Matrix theory data

The ability to combine between 2 and 5 sets of data in a matrix to generate theory data has been added via the `MatrixTheoryData` class.

As an example of how this works, consider two data sets:

* A list of int values: `[42, 2112, 2600]`
* A list of string values: `["Hello", "World"]`

You can create a matrix of these values:

```csharp
public static TheoryData<int, string> MyData =
    new MatrixTheoryData<int, string>(
        [42, 2112, 2600],
        ["Hello", "World"]
    );
```

This would create the 6 combinations of these data values for your theory:

* `42, "Hello"`
* `42, "World"`
* `2112, "Hello"`
* `2112, "World"`
* `2600, "Hello"`
* `2600, "World"`

### Capturing `Console`, `Debug`, and `Trace` output

In v2, we do not capture any output calls to `Console`, `Debug`, or `Trace`.

In v3, we have added two assembly-level attributes that you can use to capture output (and redirect it as though you had written the output via `ITestOutputHelper`), which enables capture for all tests in the assembly. These are:

* `[assembly: CaptureConsole]` can capture standard output and standard error (by default it will capture both, which you can override via the `CaptureOutput` and `CaptureError` properties on the attribute, respectively)

* `[assembly: CaptureTrace]` will capture output from `Debug` and `Trace` (note that `Debug` output is only available with debug builds of your unit tests, as the compiler filters out all `Debug` output in release builds)

For backward compatibility reasons, these are both disabled by default.

### Test pipeline startup

A new test pipeline startup capability has been added, which allows unit test developers to be able to run startup and cleanup code very early in the test pipeline. This differs from an assembly-level fixture because of how early it runs, and because it runs for both discovery and execution (whereas fixtures only run during execution). The intention with this hook is to perform some global initialization work that is needed for both discovery and execution to take place successfully.

Developers create a class which implements `ITestPipelineStartup`, and then decorate the assembly with `[assembly: TestPipelineStartup(typeof(MyStartupClass))]`. Only a single pipeline startup class is supported.

The pipeline startup class is created and `StartAsync` is called shortly after the command line options are parsed and validated, and before any substantial work is done. The `StopAsync` method is called just before the in-process runner exits. (If you've passed the `-wait` command line option, the wait will happen after `StopAsync` has completed.)

Any failure during the pipeline `StartAsync` will cause the runner to exit without performing any discovery or execution tasks. Any failure during the pipeline `StopAsync` will cause the runner to exit with a failure error code, regardless of whether all the tests discovered and/or ran successfully.

### Culture override

In v2, the culture that your unit tests ran with was the default culture of your PC.

In v3, while using the default culture of your PC remains the default behavior, you can override the culture with either a command line switch or [in your configuration file](/docs/configuration-files#culture). You can set any culture (supported by your OS) based on its [RFC 5646](https://www.rfc-editor.org/rfc/rfc5646.txt) culture name. For example, you can use `en-US` to represent English as spoken in the US.

### Repeatable randomization

The randomization of test cases in v3 is stable until you rebuild, and then the order may change. In an attempt to help developer track down issues related to the particular random order of specific test cases, we will print the randomization seed we use when [diagnostic messages are enabled](/docs/configuration-files#diagnosticMessages). The command line of the console runner has been updated to allow passing this seed value so you can attempt to reproduce the same random order that was used previously, as well as providing the seed [in your configuration file](/docs/configuration-files#seed).

### Updated theory data serialization support

#### External serialization with `IXunitSerializer`

> In v2, users who wanted to support serializable theory data items were forced to implement `IXunitSerializable` on their custom data types. Users who wanted to serialize data which they did not control, or which they did not want to add `IXunitSerializable` to, were out of luck, and forced to mark their data as non-serializable (and therefore unable to run the individual data rows in Test Explorer).
>
> In v3, we have added a way to create external serializers for theory data. To support this, developers create a class which implements `IXunitSerializer`, and then register the serializer with an assembly-level attribute.
>
> For more information, see the [documentation page](/docs/getting-started/v3/custom-serialization).

#### Updated built-in serialization support

> We have added built-in serialization support for:
>
> * [`System.Guid`](https://learn.microsoft.com/dotnet/api/system.guid)
> * [`System.Index`](https://learn.microsoft.com/dotnet/api/system.index)
> * [`System.Range`](https://learn.microsoft.com/dotnet/api/system.range)
> * [`System.Uri`](https://learn.microsoft.com/dotnet/api/system.uri)

### New report formats in the console and MSBuild runners

Two new report file formats are available:

* Common Test Report Format ([CTRF](https://ctrf.io/)), via `-ctrf` (console runner) and `Ctrf` (MSBuild runner)
* Visual Studio Test Results (TRX), via `-trx` (console runner) and `Trx` (MSBuild runner)

### Console runner (in-process and out-of-process) can list tests

In addition to running tests, you can now also list tests and test metadata rather than running tests, using the `-list` switch. This switch offers five levels of information to show (classes, methods, tests, and traits, as well as full test case information), and can output this information in either plain text form or in a machine-parseable JSON format. The information that's returned by the list request can be filtered with the same command line options that you use to filter tests for execution.

### JSON serialization

The cross-process communication between the unit tests and the runner (when using a multi-assembly runner like `xunit.v3.runner.console` or `xunit.runner.visualstudio`) is handled via JSON-encoded messages. This means that all the message classes must now support serialization.

A hand-crafted JSON serialization system has been added to `xunit.v3.common`, along with two interfaces (`IJsonSerializable` and `IJsonDeserializable`) that are implemented by messages which support serialization. This JSON serializer is very feature sparse and not guaranteed to be able to handle arbitrary JSON from outside sources; it is only intended to be used to explicitly serialize and deserialize the message classes. Using this for any other purpose is not supported.

### Third party assertion library extension points

Test results can now include a cause field, which will generally be one of `Assertion`, `Timeout`, or `Exception` (as well as two catch-all values of `Other` and `Unknown`). Any test which fails will show up by default with a failure cause of `Exception` unless the system is otherwise notified of the type of failure.

For the first party assertions, we have decorated them with interfaces that let the system know that these exceptions are a special kind of exception that indicate either an assertion failure or a timeout failure.

Third party assertion libraries can also influence the failure cause by decorating their exceptions with interfaces. These interfaces are not "hard" contracts, but rather a runtime inspection that looks for interfaces with a given name (in any namespace) to provide a hint as the kind of exception that is being thrown. This design was done so that third party assertion libraries did not need to create any hard dependencies on xUnit.net, as most are designed to be cross-test framework.

When inspecting the exception that caused the test failure, xUnit.net v3 will look at the interfaces that the exception implements. If one of the interfaces of the exception is named `IAssertionException` (in any namespace), that will cause xUnit.net to consider the failure cause to be an assertion failure; similarly, any exception where one of the interfaces is named `ITestTimeoutException` (in any namespace) will cause xUnit.net to consider that failure cause to be a timeout failure. Neither of these interfaces needs to have any methods or properties; they are merely name markers that xUnit.net can look at to help categorize the exception type.

Dynamic skipping is also done via exception. Unlike the failure exceptions, though, this is done by manipulating the _exception message_. Any exception message which starts with `$XunitDynamicSkip$` will be considered to be a dynamic skip, and the skip reason will be the remainder of the message after token. There are no interface requirements for this; any exception (including any built-in system exception) can serve this purpose so long as the message begins with the token.

The project template `xunit3-extension` illustrates one way to add a dynamic skip system (via a before/after test attribute) that will skip based on the runtime operating system. It utilizes the dynamic skip token by throwing `System.Exception` with the skip reason in the message.

### Support for Microsoft Testing Platform

The VSTest team at Microsoft has created a new testing platform to replace the existing VSTest APIs. This new platform includes features like stand-alone executables for test projects (much like xUnit.net v3), improved performance, a NuGet-based extensibility model, and will be supported by both `dotnet test` and Test Explorer inside Visual Studio. It can also provide an alternative command line experience for xUnit.net v3 test projects which can give a unified command line experience across xUnit.net v3, MSTest 3.6+, and other testing frameworks.

For more information, see the [documentation page](/docs/getting-started/v3/microsoft-testing-platform).

### Miscellaneous changes

* Several classes have had their constructors simplified by removing `IMessageSink` parameters that were previously used to send diagnostic messages to. Instead, developers can use the ambient `TestContext.Current.SendDiagnosticMessage` to simplify the sending of diagnostic messages.

* Most of the attributes designed for extensibility have had an interface extracted, so that developers can create their own base attribute types without inheriting from the ones in the framework (and it allows a single concrete attribute to be able to serve multiple purposes). The interfaces will document where they're legal (since `[AttributeUsage]` cannot be applied to an interface).

  Similarly, many of the "discoverer" types have been removed from the system, as they were in place to support the source-based discovery feature which has been removed from v3.

  For example, theory data attributes now implement `IDataAttribute`, and they're responsible for providing their own data rather than being decorated with a discoverer that finds the data. They may still use any of the base classes they previously used (like `DataAttribute` or `MemberDataAttributeBase`).

* We have added a new `AssemblyFixtureAttribute` which can be applied at the assembly-level to add an assembly-wide fixture. Fixtures at this level are created before any test is run, and cleaned up after all tests have finished running.

* `CollectionAttribute` has a new constructor that can accept a `Type`, which intended to point directly to the collection definition type. `CollectionDefinitionAttribute` has a new parameterless constructor to support this scenario.

* For .NET 6+ projects, there is a generic version of `CollectionAttribute`, where `[Collection<MyCollection>]` is equivalent to `[Collection(typeof(MyCollection))]`. _(Generic attributes are not supported in .NET Framework.)_

* `CollectionBehaviorAttribute` has a new property (`ParallelAlgorithm`) that can be used to set the parallel algorithm for the test assembly. This value can be overridden by a configuration file or a command line switch to the runner. If the value is not set, the default (`ParallelAlgorithm.Conservative`) is used.

* In v2, `Timeout` on `FactAttribute` required an async test. In v3, this requirement is not longer present; timeouts are supported now for non-async test methods.

  Note that a test which has timed out does not stop running forcefully; rather, the cancellation token available via `TestContext.Current.CancellationToken` will be signaled to indicate that the test should stop running. Regardless of whether the test method is async or not, any test should consciously check the cancellation token whenever it is reasonable & convenient, as the cancellation token will be triggered for both timeouts as well as when the user has requested to cancel the test run. We have created [an analyzer rule](/xunit.analyzers/rules/xUnit1051) which will trigger when it sees test methods calling into a method which could take a cancellation token but is not currently being passed one.

* A new `FailureCause` enum has been added, and is returned inside `ITestFailed` messages. It gives a best guess as to the cause of the test failure: assertion failure, exception thrown, or test timed out.

  This best guess is based on two contracts added to v3 which can be implemented by third party assertion libraries. Throwing an exception which implements an interface named `IAssertionException` (in any namespace) will be reported as an assertion failure; similarly, throwing an exception which implements an interface named `ITestTimeoutException` (in any namespace) will be reported as a timed-out test.

* `ITestCaseMetadata` has three new properties: `Explicit, `TestMethodParameterTypes` and `TestMethodReturnType`.

  Note that type names here are returned in [VSTest format](https://github.com/microsoft/vstest/blob/main/docs/RFCs/0017-Managed-TestCase-Properties.md), not in .NET canonical format, as they are intended to be used for VSTest and Microsoft Testing Platform consumption. We do not recommend using these properties in any other capacity.

* `ITestOutputHelper` now includes `Write` methods, in addition to the existing `WriteLine` methods. Note that test output is still buffered until it sees `Environment.NewLine`, so this does not change the contract of output reporting via `ITestOutput`, and runners will only print live test output after a full line has been written.

* `IXunitTest` has a new property, `TestMethodArguments`.

* `IXunitTestMethod` has a new property, `ReturnType`.

* `TheoryAttribute` has a new property, `SkipTestWithoutData`, that can be used to allow theories without any data to be skipped rather than failed.


## What's New in Runner Utility

### Test framework updates

In v2, the test framework (and its discoverer and executor) were invoked directly from the runner, either by virtue in being in the same process or across an Application Domain boundary. Traversing the Application Domain boundary was an expensive operation, so the v2 APIs were designed with that in mind. The objects that traversed this boundary were also potentially expensive, because they were not the original object model objects but rather abstractions designed for remoting.

In v3, there is always an in-process runner that invokes the test framework, and there are no Application Domain boundaries in play (even in .NET Framework). The objects that are passed between the in-process runner and the test framework, therefore, can be the object model objects, and there is no remoting cost involved with using those objects. More importantly, no serialization and deserialization roundtrip is required at this level, as it might have been in v2.

As a result, the test framework APIs have undergone a simplification.

**Changes to `ITestFramework`**

> * `GetDiscoverer` and `GetExecutor` now both take `Assembly` rather than `IAssemblyName` and `AssemblyName` respectively.
>
> * The `SourceInformationProvider` property has been removed. The test framework remains free to provide source information for test cases when it's known, but if the information is unknown, it can simply return `null` values for the source file and line number, and that information will be supplemented elsewhere now.
>
> * The `TestFrameworkDisplayName` property has moved from `ITestFrameworkDiscoverer` to `ITestFramework`.

**Changes to `ITestFrameworkDiscoverer`**

> * The two `Find` methods have been replaced by a single `Find` method. The two previous methods would be best described as "find all tests" and "find all tests in one type". The new `Find` method takes a optional list of types to look at, and if the list is `null`, then the discoverer should find all tests in all types.
>
> * The `Serialize` method has been removed, as serialization is not optional (due to the out-of-process requirements for third party runners). Implementations of `ITestCaseDiscovered` may choose to implement the `Serialization` property in a just-in-time form so that the actual serialization work is only done when the test case is sent across an out-of-process boundary.
>
> * The `TargetFramework` property has been removed. The in-process runner can determine this information without assistance from the test framework, since they live in the same process and were all part of the same compilation unit.
>
> * The `TestFrameworkDisplayName` property has moved from `ITestFrameworkDiscoverer` to `ITestFramework`.

**Changes to `ITestFrameworkExecutor`**

> * The `Deserialize` method has been removed, as serialization is not optional (due to the out-of-process requirements for third party runners).
>
> * The `RunAll` and `RunTests` methods have been replaced by a single `RunTestCases` method, which accepts the list of test cases to run. It is assumed that discovery will always take place before execution, due to test filtering requirements.

### Front controller updates

In v2, the front controller interface `IFrontController` was essentially a combination of `ITestFrameworkDiscoverer` and `ITestFrameworkExecutor` into a single interface. The concrete implementation `XunitFrontController` is the primary one used by runner authors to load and run unit tests without regard to which version of xUnit.net they target. It delegated to other implementations of `IFrontController` (namely `Xunit1` and `Xunit2`) after it determined which one was most appropriate. It has a standard constructor to which you pass several pieces of information, and then it does its work.

Because the API of the front controller directly mimics the test framework, it's a fairly thin wrapper (at least in the case of `Xunit2`) that doesn't offer us many opportunities for improved usability or performance.

In v3, the front controller interface is split into `IFrontController` and `IFrontControllerDiscoverer`, where the former implies the latter. The split apart discoverer is mostly an artifact of trying to continue to support source-based discovery, at least for v2 test projects (though the developer must talk directly to `Xunit2` to achieve this now).

The newly updated `XunitFrontController` in v3 has a static factory method, `Create`, which only requires one piece of information: `XunitProjectAssembly`. This consolidates the various pieces of information that were necessary in the old model into a central location that's easier to understand and pass along. You may also pass along an implementation of `ISourceInformationProvider` if you have one, to supplement discovered test cases with source information, a diagnostic message sink for reporting diagnostic messages to, and an implementation of `ITestProcessLauncher` which will be used to control how v3 test projects are launched (by default, they are launched as separate processes on the same machine).

The APIs here look slightly different now, and they've been designed to optimize the round-trip requirements across the Application Domain or process boundary. The new methods are `Find`, `FindAndRun`, and `Run`.

One of the most expensive requirements in the v2 runners was round-tripping test case objects across the Application Domain boundary. This was required because of the design that separated discovery and execution operations into separate method calls. The new `FindAndRun` API consolidates all of that into a single call across the process boundary in v3, passing the filter along so that the filtering operation takes place inside the test assembly, and not inside the runner assembly.

It is anticipated that most runner authors will choose to use `FindAndRun` as their single entry point into running tests, as this is the most optimized path. Typically only runners with separated discovery and execution processes (like Visual Studio's Test Explorer) will end up using the separate `Find` and `Run` methods.

Each of the three methods now accepts just two parameters: a message sink for the runner to receive status messages from the test run, and the settings to perform the operation in question. The settings class is a wrapper around your options (an instance of `ITestFrameworkDiscoveryOptions` and/or `ITestFrameworkExecutionOptions`) as well as the filters (if you're calling `Find` or `FindAndRun`) or the list of test cases to run (if you're calling `Run`).

Just like in v2, calling any of the methods launches the work in the background, so the method will return nearly immediately, and messages will be dispatched while everything discovers and/or runs. You will need to watch for messages that indicate you're finished (like `IDiscoveryComplete` or `ITestAssemblyFinished`).

{: .note }
As a reminder, running v1 and v2 tests, even with `xunit.v3.runner.utility`, still loads those tests into memory just like previous versions of `xunit.runner.utility`. You will generally only have success doing this when running .NET Framework projects from a .NET Framework runner, unless you are implementing the dependency resolution logic yourself. You should continue to use `AssemblyHelper.SubscribeResolveForAssembly` to assist in the .NET Framework dependency resolution; there is no equivalent provided for .NET.<br/><br/>Running v3 projects should work, regardless of whether the runner is .NET Framework or .NET, and whether the test project is .NET Framework or .NET, because the differences are isolated into their own separate process and the dependency resolution was already done by the compiler.

### Message metadata and caching

The newly restructured messages are optimized for serialization, so most metadata about a test phase is only sent in the "starting" version of the message (for example, `ITestCollectionStarting` is the only place you'll have access to the test collection metadata, which includes the the class name of the collection definition, the display name of the collection, and the traits for that collection).

Every message has a unique ID for the test phase it belongs to (so there is a `TestCollectionUniqueID` available for not only test collection messages, but for all messages further along in the pipeline, including test classes, test methods, test cases, and tests). The unique ID allows you to correlate the metadata.

In order to provide access to this metadata, we've added a `MessageMetadataCache` class. The typical way you'll use this is to add the metadata to the cache during the "starting" message (i.e., `ITestCollectionStarting`), retrieve it whenever you need it based on the unique ID, and then remove it during the "finished" message (i.e., `ITestCollectionFinished`). Remembering to remove the metadata from the cache keeps the cache as small as possible while executing various tests.

To add metadata to the cache, there are several overloads of `Set` that take the various forms of "starting" messages, and various overloads of `TryRemove` that accept the various forms of "finished" message to both retrieve and remove the metadata from the cache. To retrieve metadata, there are various `TryGetXyzMetadata` methods, each with an overload that accepts a related message interface and an overload that accepts the string unique ID.

In the test collection example, those four methods are:

```csharp
void Set(ITestCollectionStarting message);
ITestCollectionMetadata? TryGetCollectionMetadata(ITestCollectionMessage message);
ITestCollectionMetadata? TryGetCollectionMetadata(string testCollectionUniqueID, bool remove = false);
ITestCollectionMetadata? TryRemove(ITestCollectionFinished message);
```

The second overload of `TryGetCollectionMetadata` is usually not necessary, but it can serve as a replacement for both `TryGet` and `TryRemove` when you only have access to the unique ID, but don't have access to an actual message.

### Miscellaneous changes

* `AssemblyUtility.GetAssemblyMetadata` has been added which can return metadata about a test assembly, which is typically placed into the `XunitProjectAssembly` class. This is implemented using `Mono.Cecil` so that the assembly metadata can be retrieved without loading the assembly into the runner. This standardizes the logic that determines whether a test project is an xUnit.net test project, and which major version of xUnit.net it targets (1, 2, or 3).

* Added `ConsoleHelper` to wrap around any `TextWriter` (typically `System.Console.Out`) and takes responsibility for locking writes so they don't collide. It's also responsible for determining when to use `System.Console` to change colors vs. using ANSI control sequences.

* Added a `DiscoveryStarting` message as the counterpart to the existing `DiscoveryComplete` message. The front controller is responsible for sending both of these messages rather than the test framework, since the front controller may want to send repeated `Find` requests to the framework's discoverer in the context of a single "discovery" (based on the filters that were requested).

* `IRunnerLogger` gets a new method: `void WaitForAcknowledgment()`. This method is called by the in-process runner when writing messages as JSON to the console due to the v3 test process being launched with the `-automated sync` command line switch. Other consumers of runner loggers (i.e., runner reporter implementations which need to write messages for the end user to the console) may safely ignore this method.
