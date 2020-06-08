---
layout: default
title: Comparing xUnit.net to other frameworks
breadcrumb: Documentation
---

# Comparing xUnit.net to other frameworks

## Table of Contents
* [Attributes](#attributes)
* [Assertions](#assertions)

## Attributes

{: .table .smaller }
| NUnit 3.x                           | MSTest 15.x           | xUnit.net 2.x                         | Comments |
| :---------------------------------- | :-------------------- | :------------------------------------ | :------- |
| `[Test]`                            | `[TestMethod]`        | `[Fact]`                              | Marks a test method. |
| `[TestFixture]`                     | `[TestClass]`         | *n/a*                                 | xUnit.net does not require an attribute for a test class; it looks for all test methods in all public (exported) classes in the assembly.
| `Assert.That`<br>`Record.Exception` | `[ExpectedException]` | `Assert.Throws`<br>`Record.Exception` | xUnit.net has done away with the ExpectedException attribute in favor of `Assert.Throws`. See [Note 1](#note1) |
| `[SetUp]`                           | `[TestInitialize]`    | Constructor                           | We believe that use of `[SetUp]` is generally bad. However, you can implement a parameterless constructor as a direct replacement. See [Note 2](#note2) |
| `[TearDown]`                        | `[TestCleanup]`       | `IDisposable.Dispose`                 | We believe that use of `[TearDown]` is generally bad. However, you can implement `IDisposable.Dispose` as a direct replacement. See [Note 2](#note2) |
| `[OneTimeSetUp]`                    | `[ClassInitialize]`   | `IClassFixture<T>`                    | To get per-class fixture setup, implement `IClassFixture<T>` on your test class. See [Note 3](#note3) |
| `[OneTimeTearDown]`                 | `[ClassCleanup]`      | `IClassFixture<T>`                    | To get per-class fixture teardown, implement `IClassFixture<T>` on your test class. See [Note 3](#note3) |
| *n/a*                               | *n/a*                 | `ICollectionFixture<T>`               | To get per-collection fixture setup and teardown, implement `ICollectionFixture<T>` on your test collection. See [Note 3](#note3) |
| `[Ignore("reason")]`                | `[Ignore]`            | `[Fact(Skip="reason")]`               | Set the Skip parameter on the `[Fact]` attribute to temporarily skip a test. |
| `[Property]`                        | `[TestProperty]`      | `[Trait]`                             | Set arbitrary metadata on a test |
| `[Theory]`                          | `[DataSource]`        | `[Theory]`<br>`[XxxData]`             | Theory (data-driven test). See [Note 4](#note4) |

## Attribute Notes

<a name="note1"></a>**Note 1:** Long-term use of `[ExpectedException]` has uncovered various problems with it. First, it doesn't specifically say which line of code should throw the exception, which allows subtle and difficult-to-track failures that show up as passing tests. Second, it doesn't offer the opportunity to fully inspect details of the exception itself, since the handling is outside the normal code flow of the test. `Assert.Throws` allows you to test a specific set of code for throwing an exception, and returns the exception during success so you can write further asserts against the exception instance itself.

<a name="note2"></a>**Note 2:** The xUnit.net team feels that per-test setup and teardown creates difficult-to-follow and debug testing code, often causing unnecessary code to run before every single test is run. For more information, see <http://jamesnewkirk.typepad.com/posts/2007/09/why-you-should-.html>.

<a name="note3"></a>**Note 3:** xUnit.net provides a new way to think about per-fixture data with the use of the `IClassFixture<T>` and `ICollectionFixture<T>` interfaces. The runner will create a single instance of the fixture data and pass it through to your constructor before running each test. All the tests share the same instance of fixture data. After all the tests have run, the runner will dispose of the fixture data, if it implements `IDisposable`. For more information, see [Shared Context](shared-context.html).

<a name="note4"></a>**Note 4:** xUnit.net ships with support for data-driven tests call Theories. Mark your test with the `[Theory]` attribute (instead of `[Fact]`), then decorate it with one or more `[XxxData]` attributes, including `[InlineData]` and `[MemberData]`. For more information, see [Getting Started](getting-started-desktop.html).

## Assertions

NUnit uses a [Constraint Model](https://github.com/nunit/docs/wiki/Constraint-Model). All the assertions start with `Assert.That` followed by a constraint. In the table below, we compare NUnit constraints, MSTest asserts, and xUnit asserts.

{: .table .smaller }
| NUnit 3.x (Constraint)      | MSTest 15.x           | xUnit.net 2.x      | Comments |
| :------------------------- | :-------------------- | :----------------- | :------- |
| `Is.EqualTo`               | `AreEqual`            | `Equal`            | MSTest and xUnit.net support generic versions of this method |
| `Is.Not.EqualTo`           | `AreNotEqual`         | `NotEqual`         | MSTest and xUnit.net support generic versions of this method |
| `Is.Not.SameAs`            | `AreNotSame`          | `NotSame`          | |
| `Is.SameAs`                | `AreSame`             | `Same`             | |
| `Does.Contain`             | `Contains`            | `Contains`         | |
| `Does.Not.Contain`         | `DoesNotContain`      | `DoesNotContain`   | |
| `Throws.Nothing`           | *n/a*                 | *n/a*              | Ensures that the code does not throw any exceptions. See [Note 5](#note5) |
| *n/a*                      | `Fail`                | *n/a*              | xUnit.net alternative: `Assert.True(false, "message")` |
| `Is.GreaterThan`           | *n/a*                 | *n/a*              | xUnit.net alternative: `Assert.True(x > y)` |
| `Is.InRange`               | *n/a*                 | `InRange`          | Ensures that a value is in a given inclusive range |
| `Is.AssignableFrom`        | *n/a*                 | `IsAssignableFrom` | |
| `Is.Empty`                 | *n/a*                 | `Empty`            | |
| `Is.False`                 | `IsFalse`             | `False`            | |
| `Is.InstanceOf<T>`         | `IsInstanceOfType`    | `IsType<T>`        | |
| `Is.NaN`                   | *n/a*                 | *n/a*              | xUnit.net alternative: `Assert.True(double.IsNaN(x))` |
| `Is.Not.AssignableFrom<T>` | *n/a*                 | *n/a*              | xUnit.net alternative: `Assert.False(obj is Type)` |
| `Is.Not.Empty`             | *n/a*                 | `NotEmpty`         | |
| `Is.Not.InstanceOf<T>`     | `IsNotInstanceOfType` | `IsNotType<T>`      | |
| `Is.Not.Null`              | `IsNotNull`           | `NotNull`          | |
| `Is.Null`                  | `IsNull`              | `Null`             | |
| `Is.True`                  | `IsTrue`              | `True`             | |
| `Is.LessThan`              | *n/a*                 | *n/a*              | xUnit.net alternative: `Assert.True(x < y)` |
| `Is.Not.InRange`           | *n/a*                 | `NotInRange`       | Ensures that a value is not in a given inclusive range |
| `Throws.TypeOf<T>`         | *n/a*                 | `Throws<T>`         | Ensures that the code throws an exact exception |

## Attribute Notes

<a name="note5"></a>**Note 5:** Older versions of xUnit.net provided an `Assert.DoesNotThrow` which was later removed. It revealed itself to be an anti-pattern of sorts; every line of code is itself an implicit "does not throw" check, since any thrown exceptions would cause the test to fail. Simply "not throwing" is not generally a sufficient validation, so it would be expected that additional assertions would exist.

## Sources

* [https://github.com/nunit/nunit](https://github.com/nunit/nunit)
* [https://msdn.microsoft.com/en-us/library/ms243147.aspx](https://msdn.microsoft.com/en-us/library/ms243147.aspx)
* [https://github.com/xunit/xunit](https://github.com/xunit/xunit)
