---
layout: default
title: Why Did We Build xUnit 1.0?
breadcrumb: Documentation
---
# Why Did we Build xUnit 1.0?

*This originally appeared on [Jim's blog](http://jamesnewkirk.typepad.com/posts/2007/09/announcing-xuni.html), and has been updated for the 1.0 release of xUnit.net.*

In the 5 years since the release of NUnit 2.0, there have been millions of lines of code written using the various unit testing frameworks for .NET. About a year ago it became clear to myself and Brad Wilson that there were some very clear patterns of success (and failure) with the tools we were using for writing tests. Rather than repeating guidance about "do X" or "don't do Y", it seemed like it was the right time to reconsider the framework itself and see if we could codify some of those rules.

Additionally, the .NET framework itself has evolved a lot since its v1 release in early 2002. Being able to leverage some of the new framework features can help us write clearer tests.

Another aspect of change that we wanted to affect was bringing the testing framework more closely in line with the .NET platform. Many of the decisions we made, which we enumerate below, were driven by this desire. We wanted an architecture which is built for programmer testing (specifically Test-Driven Development), but can also be very easily extended to support other kinds of testing (like automated acceptance tests).

Finally, there have been advances in other unit test library implementations that have not really surfaced in the .NET community.

While any one of these reasons would not necessarily have been sufficient to create a new testing framework, the combination of them all made us want to undertake a new project: xUnit.net.

## Lessons Learned
* **Single Object Instance per Test Method.** Much has been written about why this improves test isolation. In xUnit.net we create a new instance per test. For more information, see <http://martinfowler.com/bliki/JunitNewInstance.html>.
* **No `[SetUp]` or `[TearDown]`.** I blogged recently about some of the problems related to SetUp/TearDown. xUnit.net does not have any built-in support for this capability. For more information, see <http://jamesnewkirk.typepad.com/posts/2007/09/why-you-should-.html>
* **No `[ExpectedException]`.** Rather than decorating a method with an attribute, we have returned to the old JUnit style of Assert.Throws for expected exceptions. This helps two major issues: 1. With `[ExpectedException]` it's possible to hide real errors when the wrong method call throws an exception, and 2. Allows your tests to continue to obey the [Arrange-Act-Assert (or "3A") pattern](http://xp123.com/articles/3a-arrange-act-assert/), as coined by William Wake.
* **Aspect-Like Functionality.** End users extended NUnit and MbUnit with cross-cutting concerns that could be attached to test methods (an example is automatically rolling back changes made to a database during the test). This made the tests simpler to write and allowed more consistent usage of the cross-cutting operations. xUnit.net makes it very simple to create such operations and attach them to test methods.
* **Reducing the number of custom attributes.** Sometimes, the excessive use of attributes can make you feel like you've diverged far from the underlying language. xUnit.net removed some attributes from the framework, instead relying on language features to provide similar functionality:
 * `[TestFixture]` was removed entirely; tests can be in any public class. Test methods can be static or instance, to better facilitate testing with F#.
 * `[Ignore]` is expressed using the Skip= parameter on `[Fact]`.
 * `[SetUp]` and `[TearDown]` are removed in favor of constructors and IDisposable.
 * `[ExpectedException]` was replaced with Assert.Throws (or Record.Exception, which provides better adherence to the 3A pattern).
 * `[TestFixtureSetup]` and `[TestFixtureTearDown]` are removed in favor of implementing reusable fixture data classes, which are attached to test classes by having them implement `IUseFixture<T>`.

## Language Features

* **Use of Generics.** The addition to generics to .NET 2.0 allowed much more concise assertions, allowing us to add type-specific comparison support for the more common asserts (like `Equal` and `NotEqual`)
* **Anonymous Delegates**. Support for anonymous delegates in .NET 2.0 made the syntax for Assert.Throws much more compact and readable. Here are two examples of `Assert.Throws`:

* `Assert.Throws<InvalidOperationException>(delegate { operation(); }); // VS 2005`
* `Assert.Throws<InvalidOperationException>(() => operation()); // VS 2008`

## Test Runners
For 1.0, we are shipping several runners:

* A console-based test runner
* An MSBuild test runner
* A TestDriven.net 2.x test runner (inside Visual Studio)
* A ReSharper 3.1 test runner (inside Visual Studio)
* An experimental GUI-based test runner


## Extensibility

* **Assert extensibility.** Through the use of custom comparers (that implement `IComparer<T>`), you can extend the concepts of `Equal`, `NotEqual`, `Contains`, `DoesNotContain`, `InRange`, and `NotInRange` for your tests.
* **Test method extensibility.** The definition of how to run a test method can be extended. There are two example of this: the first, in xunitext.dll, is the `[Theory]` attribute which allows data-driven tests; the second, in the samples, is the `[RepeatTest]` attribute which runs a test method multiple times in a row. For more information on data theories, see <http://shareandenjoy.saff.net/2006/12/new-paper-practice-of-theories.html>.
* **Test class extensibility**. The definition of run to run a test class can be extended. There is an example of this in xunitext.nunit.dll, the `[RunWithNUnit]` attribute which allows you to have mixed xUnit.net and NUnit tests in the same assembly, all executable by any xUnit.net runner.
* **Version independent runner support.** All of the runners in v1.0 (except the ReSharper runner) are written in a version-independent manner. This means that they have no dependencies on xunit.dll itself, and so a single copy of the runner should be able to run any current or future versions of tests. They will utilize the version of xunit.dll that is present in the same directory as the assembly under test. Third party runner authors are highly encouraged to use the version independent runner support library (xunit.runner.utility.dll).

We also provide a more complete [comparison](comparisons.html) of xUnit.net to other predominant test frameworks on .NET. The samples include illustrations of several key concepts, especially with extensibility.
