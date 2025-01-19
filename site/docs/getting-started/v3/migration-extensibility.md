---
layout: default
title: Migrating from v2 to v3 [Extensibility authors]
breadcrumb: Documentation
---

# Migrating from v2 to v3 [Extensibility authors]

## As of: 2025 January 1 (`1.0.0`)

This migration guide aims to be a focused guide for extensibility authors who want to migrate their xUnit.net extensions from v2 to v3. This guide is focused on the extensibility experience, and assumes you are already familiar with the [migration guidance for unit test authors](migration) already. You may also want to read the "[What's New in v3](whats-new)" document to know what new features have been added to v3, that you may way to include in your extensibility.

In this document, we will split guidance based on those writing extensions to the core framework ("Core Extensibility") and those who are writing test runners ("Runner Extensibility"). For developers who are extending the assertion library, there should be little to no change for you other than updating your package names (from `xunit.assert` or `xunit.assert.source` to `xunit.v3.assert` or `xunit.v3.assert.source` respectively).

{: .note }
This documentation is a preliminary work in progress, and will be improved over time. If you find yourself in need of extensibility help that isn't yet provided here, please [open an issue in our primary repository](https://github.com/xunit/xunit/issues) so that we can prioritize creating that documentation for you.

The current builds are:

{: .table .latest }
Package                     | NuGet Version                                                                                                                               | [CI Version](/docs/using-ci-builds)
--------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------
`xunit.v3.*`                | [![](https://img.shields.io/nuget/vpre/xunit.v3.svg?logo=nuget)](https://www.nuget.org/packages/xunit.v3)                                   | [![](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fxunit%2Fxunit%2Fshield%2Fxunit.v3%2Flatest&color=f58142)](https://feedz.io/org/xunit/repository/xunit/packages/xunit.v3)
`xunit.analyzers`           | [![](https://img.shields.io/nuget/vpre/xunit.analyzers.svg?logo=nuget)](https://www.nuget.org/packages/xunit.analyzers)                     | [![](https://img.shields.io/badge/endpoint.svg?url=https%3A%2F%2Ff.feedz.io%2Fxunit%2Fxunit%2Fshield%2Fxunit.analyzers%2Flatest&color=f58142)](https://feedz.io/org/xunit/repository/xunit/packages/xunit.analyzers)

Note that while we attempt to ensure that CI builds are always usable, we cannot make guarantees. If you come across issues using a CI build, please [let us know](https://github.com/xunit/xunit/issues)!

## Table of Contents

* [Core Extensibility](#core-extensibility)
  * [Migrating to v3 packages](#migrating-to-v3-packages)
  * [Package naming](#package-naming)
  * [Attributes and interfaces](#attributes-and-interfaces)
  * [Discoverers](#discoverers)
  * [FactAttribute and IXunitTestCaseDiscoverer](#factattribute-and-ixunittestcasediscoverer)
  * [ITestCase](#itestcase)
  * [IXunitTestCase](#ixunittestcase)
* [Runner Extensibility](#runner-extensibility)

## Core Extensibility

### Migrating to v3 packages

Developers who are writing core framework extensibility will need to upgrade from referencing `xunit.extensibility.core` and/or `xunit.extensibility.execution` to referencing `xunit.v3.extensibility.core`, as in v3 there is no more library split between writing tests and writing test frameworks.

In v3, this package has a single target framework: .NET Standard 2.0 (`netstandard2.0`). All target platforms are addressed by this single target framework. If your extensibility library has need for target framework splits (like .NET Framework vs. .NET), you can do support that split transparently through our single `netstandard2.0` library. It's important to note that while `netstandard2.0` claims to support .NET Framework versions prior to 4.7.2, it was later discovered that there were edge cases where that compatibility did not work on older versions of .NET Framework. As such, we only officially support .NET Framework 4.7.2 or later, and strongly recommend you do the same. We will be unable to provide any support with issues that arise on versions of .NET Framework prior to 4.7.2.

### Package naming

We have migrated to new package names in v3, for the reasons described in the [unit test migration guide](migration#why-did-we-change-the-package-names):

> We changed the package naming scheme from `xunit.*` to `xunit.v3.*` for two primary reasons and one secondary reason:
>
> * We wanted users to make a conscious choice to upgrade and understand what the scope of that work is, rather than being offered a NuGet package upgrade in Visual Studio and then have everything be broken without being told why.
> * We have frequently been asked to observe SemVer for our package versions, which has been impossible previously. Our package naming and versioning system predates SemVer, and trying to adopt it after the fact would be painful. The `2` in the `2.x.y` package versioning scheme implied a _**product version**_ but it was living in the major version of the package. The new package name allows the v3 _**product version**_ to live in the package name instead of the major version, and this allows us to evolve those package versions according to SemVer without implying a new production version has been released.
>
> The secondary reason was:
>
> * Some packages have been merged, and some new intermediate packages have been introduced. We previously tried the "upgrade an obsoleted package" strategy from v1 -> v2 with the `xunit.extensions` package and found that process less than ideal for most users. This is not an area where NuGet is particularly helpful. We would've preferred that we could have automatically removed `xunit.extensions` rather than having a v2 version in place with no code inside as a dead reference. By having users follow this migration guide, we can clearly tell them which packages changed and which should be removed.

For extensibility package developers, you have (at least) two choices for how to evolve your dependencies and package names:

* You can use your existing package name and just pick a new version that takes a dependency on `xunit.v3.extensibilty.core` rather than `xunit.extensibility.core` and/or `xunit.extensibility.execution`. This means as users pick up new versions of your existing packages, you'll _**also**_ be expecting them to upgrade to xUnit.net v3.

* You can use a new package name that indicates an updated extension for v3, while leaving the old name in place to support v2. This means as users upgrade up xUnit.net v3, they'll also need to find your new matching package.

For the reasons we've listed above, we feel like this first option is the less ideal choice. We believe that following our strategy of new package names is probably the one that will lead users to the most success, because as they're consciously choosing to upgrade xUnit.net from v2 to v3, they can also be consciously thinking about what that means for upgrading their extensions.

However, you know your users best, and there's no one right answer here. It's your decision which strategy makes the best sense for you and your users.

### Attributes and interfaces

One major change we made with respect to attributes and extensibility was to introduce interfaces wherever possible. When developers wants to implement a piece of functionality as an attribute, they can use whatever base class they wish (as long as it derives from `Attribute`, since that's a .NET requirement), so long as the attribute implements the interface.

For example: when overriding `[Fact]` with a custom attribute, you are no longer required to derive from `FactAttribute`; instead, the only requirement is that you implement `IFactAttribute`. This gives greater flexibility in implementation because you're not limited by the design decisions of the concrete attributes. For example, `FactAttribute` implements the `Skip` property as read/write, but `IFactAttribute` only requires that you provide read access to the value. This would allow you to create your own test method decoration that doesn't necessarily allow `Skip` to be directly set by the end user.

One important thing to note is that `[AttributeUsage]` attributes can only be placed on classes that derive from `Attribute`, so we cannot enforce the `[AttributeUsage]` for attributes that implement interfaces. Instead, we have added notes in the XML documentation for these interfaces to indicate where the attributes are legal, which should guide your application of `[AttributeUsage]` on your custom attributes. For example, the `IFactAttribute` documentation for 1.0.0 reads as follows:

```xml
/// <summary>
/// Attribute that is applied to a method to indicate that it is a test method that should
/// be run by the default test runner. Implementations must be decorated by
/// <see cref="XunitTestCaseDiscovererAttribute"/> to indicate which class is responsible
/// for converting the test method into one or more tests.
/// </summary>
/// <remarks>The attribute can only be applied to methods, and only one attribute is allowed.</remarks>
```

The documentation indicates that your custom attribute should end up with decorations like this:

```csharp
[XunitTestCaseDiscoverer(typeof(MyCustomFactDiscoverer))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MyCustomFactAttribute : Attribute, IFactAttribute
{
    // ...
}
```

As we describe some of the common extensibility points, we will point out when these interfaces are available as alternatives to using our attributes as base classes.

### Discoverers

With xUnit.net v2, we supported source-based test discovery in our runner APIs. One big way this surfaced in our extensibility APIs is that we frequently created a "discoverer" abstraction whose job it was to consume our reflection abstractions and produced results, even when compiled code wasn't available.

We have removed this functionality in xUnit.net v3, due to the move to out-of-process test projects. This means we also removed our reflection abstractions (types like `IAssemblyInfo`, `ITypeInfo`, `IMethodInfo`, etc.) and instead replaced them with the actual reflection types from .NET (types like `Assembly`, `Type`, `MethodInfo`, etc.). This also meant that many of our discoverers (though not all) have been removed, and in many cases, the attributes themselves are responsible for being self-describing. It also means we simplified type registration of things to be able to use `typeof(T)` rather than the dual-string type-name and assembly-name combinations that were frequently used in v2.

### FactAttribute and IXunitTestCaseDiscoverer

In v2, developers who wanted to redefine unit test discovery and/or execution would, at a minimum, derive a new attribute from `FactAttribute`. If they wanted to do custom discovery logic, they would also create a discoverer that implemented `IXunitTestCaseDiscover` and decorate their custom `FactAttribute` with `[XunitTestCaseDiscoverer]` to point at the discoverer.

In v3, the same strategy is generally used, though as noted above you may implement `IFactAttribute` rather than directly deriving from `FactAttribute` if you prefer. The types `FactAttribute`, `IXunitTestCaseDiscover`, and `XunitTestCaseDiscovererAttribute` have all moved namespaces (from `Xunit.Sdk` to `Xunit.v3`). `XunitTestCaseDiscovererAttribute` now takes a single `Type` pointing to your discoverer rather than the dual-string registration system from v2.

Often, providing a custom `FactAttribute` and discoverer meant that you might be providing custom test cases during discovery as well.

### ITestCase

Test cases are the fundamental unit of discovery and execution in xUnit.net. The job of discovery is create test cases, which are then run during execution.

The most basic version of a test case implements `ITestCase`. In v2, this interface lives in namespace `Xunit.Abstractions`; in v3, it lives in namespace `Xunit.Sdk`. Implementing this interface meant being able to describe the test case's metadata (things like its display name) as well as where it lives in the hierarchy (which test method it belongs to). Because test cases are passed across process boundaries, they must be serializable, so the `ITestCase` interface inherits `IXunitSerializable`.

Here is as comparison between the v2 and v3 version of `ITestCase`:

{: .table .left }
v2                    | v3                                      | Description
--------------------- | --------------------------------------- | -----------
`DisplayName`         | `TestCaseDisplayName`                   | The display name of the test case
                      | `Explicit`                              | A boolean flag indicating if the test case is marked explicit or not
`SkipReason`          | `SkipReason`                            | The skip reason (for a statically skipped test)
`SourceInformation`   | `SourceFilePath` and `SourceLineNumber` | The source file and line number where the test resides (if known)
                      | `TestClass`                             | An object that points to the test class (if the test case comes from a class)
                      | `TestClassMetadataToken`                | The reflection metadata token for the test class, as provided by `Type.MetadataToken`
                      | `TestClassName`                         | The fully qualified type name of the test class (if the test case comes from a class)
                      | `TestClassNamespace`                    | The namespace of the test class (if the test case comes from a class)
                      | `TestClassSimpleName`                   | The simple name of the test class, as provided by the `Type.ToSimpleName()` extension method
                      | `TestCollection`                        | An object that points to the test collection the test case belongs to
`TestMethod`          | `TestMethod`                            | An object that points to the test method (if the test case comes from a method)
`TestMethodArguments` |                                         | The arguments that will be passed to the test method when executed
                      | `TestMethodMetadataToken`               | The reflection metadata token for the test method, as provided by `MethodInfo.MetadataToken`
                      | `TestMethodName`                        | The name of the test method (if the test case comes from a method)
                      | `TestMethodParameterTypesVSTest`        | The VSTest-formatted parameter types of the test method, as provided by the `Type.ToVSTestTypeName()` extension method
                      | `TestMethodReturnTypeVSTest`            | The VSTest-formatted return type of the test method, as provided by the `Type.ToVSTestTypeName()` extension method
`Traits`              | `Traits`                                | The collection of traits (name/value pairs) of the test case
`UniqueID`            | `UniqueID`                              | The unique ID of the test case

As functionality grew from v2 to v3, so too did the requirements for implementing `ITestCase` grow. In v3, `ITestCase` is pared down to just `TestClass`, `TestCollection`, and `TestMethod`, which represent the object model for the test case (its parents in the object model hierarchy). Every other property here is actually on `ITestCaseMetadata`, which `ITestCase` inherits from; the metadata represents the flattened information about the test case (metadata which is represented primary by intrinsic types values or collections of intrinsic type values). You will frequently see that some APIs require a full `ITestCase`, whereas others which operate only on the metadata can accept `ITestCaseMetadata`.

The `ITestCase` base interface represents the lowest level definition of what it might mean to have something runnable that could produce test results. As you'll note with the test class and test method being optional, this could include test definitions that are not based on C# classes (for example, if you have a non-CLR language to write tests in, like [Gherkin](https://cucumber.io/docs/gherkin/)).

{: .note }
The two VSTest named properties may seem unusual, but they are required for proper source mapping by VSTest (via `xunit.runner.visualstudio`) in Test Explorer. We have provided reflection extension methods will can convert a `Type` into the correctly formatted VSTest type names, so that you need not be concerned with the exact mapping process. Unfortunately that logic must live here, rather than in `xunit.runner.visualstudio`, since the reflection information about the test method is only available here, and not available there.

### IXunitTestCase

The test cases generated by the built-in `FactAttribute`-based attributes and their associated discoverers generated test cases which include a more rich description of functionality. This richer interface is `IXunitTestCase`, and it derives from `ITestCase`.

In v2, this interface lives in namespace `Xunit.Sdk`; in v3, it lives in namespace `Xunit.v3`. Much like with `ITestCase`, the version in v3 includes new properties:

{: .table .left }
v2                        | v3                                      | Description
------------------------- | --------------------------------------- | -----------
`InitializationException` |                                         | Contains a reference to any exception that occurred during initialization (in v3, this is stored in the execution context)
`Method`                  | `TestMethod.Method`                     | Points to the CLR `MethodInfo` for the test method
                          | `SkipExceptions`                        | _(New in 2.0.0)_ A list of exception types that, if thrown, will cause the test case to skip rather than fail
                          | `SkipReason`                            | Redefined here because `ITestCase.SkipReason` implies something which is always statically skipped
                          | `SkipType`                              | The optional type where `SkipUnless` or `SkipWhen` live
                          | `SkipUnless`                            | A pointer to a static property which is used to determine if the test is skipped at runtime (`false` skips the test)
                          | `SkipWhen`                              | A pointer to a static property which is used to determine if the test is skipped at runtime (`true` skips the test)
`Timeout`                 | `Timeout`                               | The timeout for the test execution, in seconds

Additionally, the v2 `IXunitTestCase` was always "self-executing"; that is, developers were forced to implement the `RunAsync()` method in order to run the test case. In v3, `IXunitTestCase` test cases are not self-executing by default; instead, they can implement `ISelfExecutingXunitTestCase` to provide this functionality. Any v3 test case which does not implement this interface will use the standard test execution pipeline.

In order to implement this default execution pipeline, instead we ask implementers of `IXunitTestCase` in v3 to implement `CreateTests()`, which return a collection of 1 or more `IXunitTest` implementations which represent the tests that will be run by the test case. For fact-style tests (and for theory-style tests with theory data pre-enumeration enabled), there is a 1:1 mapping between test case and test; the test case here represents a single executable test, either because it's fact-style or because it represents a single data row. For theory-style tests with theory data pre-enumeration disabled (or for theories with non-serializable theory data), there may be a 1:many mapping between test case and test, where each data row is its own test within the group of the test case (where there is just one test case for the whole test method).

Two execution hooks are also provided here: `PreInvoke()` and `PostInvoke()` will be called by the standard execution pipeline just before the test case starts executing, and just after the test case has finished executing (these are only called once for the test case, regardless of how many tests the test case represents).

## Runner Extensibility

The runner extensibility documentation is still forthcoming.
