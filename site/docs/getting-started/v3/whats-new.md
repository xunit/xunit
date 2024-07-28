---
layout: default
title: What's New in v3
breadcrumb: Documentation
---

# What's New in v3

## As of: 2024 July 27 (`0.2.0-pre.59`)

This guide aims to be a comprehensive list of the new features added to v3, written for existing developers who are using v2.

In addition to this new features document (which only covers new features in v3), we have a parallel document which covers [migrating from v2 to v3](migration). The "Migrating from v2 to v3" document describes changes that developers will need to understand when porting their v2 projects to v3, as well as when starting new v3 projects armed with their v2 experience.

## Table of Contents

* [What's New in the Assertion Library](#whats-new-in-the-assertion-library)
* [What's New in the Core Framework](#whats-new-in-the-core-framework)
* [What's New in Runner Utility](#whats-new-in-runner-utility)

## What's New in the Assertion Library

Items here are related to the assertion library, from the `xunit.v3.assert` and `xunit.v3.assert.source` NuGet packages.

### New assertions for everybody

#### Dynamic skipping

> Three new assertions have been added to support dynamically skipping a test at runtime.
>
> * `Assert.Skip(string message)`
> * `Assert.SkipUnless(bool condition, string reason)` will dynamically skip the test only if `condition` is `false`
> * `Assert.SkipWhen(bool condition, string reason)` will dynamically skip the test only if `condition` is `true`

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

## What's New in the Core Framework

Items here are related to the core framework used for writing tests, from the `xunit.v3.common` and `xunit.v3.core` NuGet packages.

### Dynamically skippable tests

We have added the ability to dynamically skip tests via the `[Fact]` and `[Theory]` attributes, in addition to the `Assert.Skip` family of assertions mentioned above.

You may set either `SkipUnless` or `SkipWhen` (but not both) to point at a public static property on the test class which returns `bool`; to skip the test, return `false` for `SkipUnless` or `true` for `SkipWhen`. You may also place the public static property on another class by setting `SkipType`. Note that setting both `SkipUnless` and `SkipWhen` will result in a runtime test failure.

During discovery, tests which have `Skip` set along with one of `SkipUnless` or `SkipWhen` will not show up as statically skipped tests; instead, the property value will be inspected at runtime during test execution to determine if it should be skipped at runtime. This means that, unlike statically skipped tests, the entire test pipeline is run for such tests, including `BeforeAfterTestAttribute` instances, test class creation, and test class disposal.

### Explicit tests

We have added support for explicit tests; that is: tests which are normally not run unless it is requested that all explicit tests run (such as a runner command line switch) or through a gesture that indicates that the user wants to run a specific explicit test (such as asking to run it inside Test Explorer).

A test is marked as explicit through the new `Explicit` property on the `[Fact]` and `[Theory]` attributes. Tests which aren't run (because they don't match the requested explicit option) are reported in runners as "not run".

### Test context

We have added a `TestContext` class which is designed to:

* Provide (via several properties) information about the current state of the test pipeline.
* Provide (via `.CancellationToken`) a cancellation token that can be passed to downstream methods in your test to help cancellation occur sooner, as well as (via `.CancelCurrentTest`) to attempt to cancel the execution of the current test.
* Provide (via `.SendDiagnosticMessage`) the ability to send diagnostic messages from anywhere.
* Store and retrieve (via `.KeyValueStorage`) information that can be used to communicate between different stages of the test pipeline
* Add (via `.AddAttachment`) attachments to the test results, that will be recorded into the various report formats. Attachments are named, and may container either plain-text string content or binary content (with an associated MIME media type).
* Add (via `.AddWarning`) warnings to test results, which will be reported to the test runner as well as recorded into the various report formats

Access to the test context is available in two ways:

* You can access it from anywhere via the static `TestContext.Current`.
* You can access it via `ITestContextAccessor`, which can be injected into a test class's constructor alongside fixtures. _(There is an [open issue](https://github.com/xunit/xunit/issues/1738) related to expanding dependency injection support in the framework itself, which would include allowing `ITestContextAccessor` to be injected in other places besides test classes.)

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

We have introduced a new base class `TheoryDataRow` for untyped data rows, and 1-10 type argument generic versions of `TheoryDataRow<>` for strongly typed data rows. You can use a parameter-setting pattern for these theory data rows, like:

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

### Test pipeline startup

A new test pipeline startup capability has been added, which allows unit test developes to be able to startup and cleanup code very early in the test pipeline. This differs from an assembly-level fixture because of how early it runs, and because it runs for both discovery and execution (whereas fixtures only run during execution). The intention with this hook is to perform some global initialization work that is needed for both discovery and execution to take place successfully.

Developers create a class which implements `ITestPipelineStartup`, and then decorate the assembly with `[assembly: TestPipelineStartup(typeof(MyStartupClass))]`. Only a single pipeline startup class is supported.

The pipeline startup class is created and `StartAsync` is called shortly after the command line options are parsed and validated, and before any substantial work is done. The `StopAsync` method is called just before the in-process runner exits. (If you've passed the `-wait` command line option, the wait will happen after `StopAsync` has completed.)

Any failure during the pipeline `StartAsync` will cause the runner to exit without performing any discovery or execution tasks. Any failure during the pipeline `StopAsync` will cause the runner to exit with a failure error code, regardless of whether all the tests discovered and/or ran successfully.

### JSON serialization

The cross-process communication between the unit tests and the runner (when using a multi-assembly runner like `xunit.v3.runner.console` or `xunit.runner.visualstudio`) is handled via JSON-encoded messages. This means that all the message classes must now support serialization.

A hand-crafted JSON serialization system has been added to `xunit.v3.common`, along with two interfaces (`IJsonSerializable` and `IJsonDeserializable`) that are implemented by messages which support serialization. This JSON serializer is very feature sparse and not guaranteed to be able to handle arbitrary JSON from outside sources; it is only intended to be used to explicitly serialize and deserialize the message classes. Using this for any other purpose is not supported.

### Miscellaneous changes

* Several classes have had their constructors simplified by removing `IMessageSink` parameters that were previously used to send diagnostic messages to. Instead, developers can use the ambient `TestContext.Current.SendDiagnosticMessage` to simplify the sending of diagnostic messages.

* Most of the attributes designed for extensibility have had an interface extracted, so that developers can create their own base attribute types without inheriting from the ones in the framework (and it allows a single concrete attribute to be able to serve multiple purposes). The interfaces will document where they're legal (since `[AttributeUsage]` cannot be applied to an interface).

  Similarly, many of the "discoverer" types have been removed from the system, as they were in place to support the source-based discovery feature which has been removed from v3. Some of the attributes

  For example, theory data attributes now implement `IDataAttribute`, and they're responsible for providing their own data rather than being decorated with a discoverer that finds the data. They may still use any of the base classes they previously used (like `DataAttribute` or `MemberDataAttributeBase`).

* We have added a new `AssemblyFixtureAttribute` which can be applied at the assembly-level to add an assembly-wide fixture. Fixtures at this level are created before any test is run, and cleaned up after all tests have finished running.

* `CollectionAttribute` has a new constructor that can accept a `Type`, which intended to point directly to the collection definition type. `CollectionDefinitionAttribute` has a new parameterless constructor to support this scenario.

* `CollectionBehaviorAttribute` has a new property (`ParallelAlgorithm`) that can be used to set the parallel algorithm for the test assembly. This value can be overridden by a configuration file or a command line switch to the runner. If the value is not set, the default (`ParallelAlgorithm.Conservative`) is used.

* For .NET 6+ projects, there is a generic version of `CollectionAttribute`, where `[Collection<MyCollection>]` is equivalent to `[Collection(typeof(MyCollection))]`. _(Generic attributes are not supported in .NET Framework.)_

* A new `FailureCause` enum has been added, and is returned inside `ITestFailed` messages. It gives a best guess as to the cause of the test failure: assertion failure, exception thrown, or test timed out.

  This best guess is based on two contracts added to v3 which can be implemented by third party assertion libraries. Throwing an exception which implements an interface named `IAssertionException` (in any namespace) will be reported as an assertion failure; similarly, throwing an exception which implements an interface named `ITestTimeoutException` (in any namespace) will be reported as a timed-out test.

## What's New in Runner Utility

_Content coming soon_
