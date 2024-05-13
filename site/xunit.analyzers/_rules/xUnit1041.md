---
title: xUnit1041
description: Fixture arguments to test classes must have fixture sources
category: Usage
severity: Warning
---

## Cause

Test class constructors which indicate fixture data must have a source for those fixtures.

## Reason for rule

Fixture data must come from a fixture source. Any constructor argument which is fixture data, but does
not have an associated fixture source, is an error condition and every test in the class will fail
at runtime.

There are three fixture sources:

- `IClassFixture<>` can decorate the test class or the collection definition class
- `ICollectionFixture<>` can decorate the collection definition class
- `AssemblyFixtureAttribute` can decorate the assembly (for v3 only)

For more information on fixtures and shared context, please see [the documentation](/docs/shared-context).

_Note: Collection definition classes must be defined in the same assembly as the test. You may get this
analyzer on your source code if you've mistakenly put your collection definition class into the wrong
assembly._

_Note: Third party fixture source libraries are not supported by this analyzer. For projects which use
third party collection sources, you should disable this rule (conditionally or globally). Ideally third
party fixture source libraries should provide their own customization of `xUnit1041` that includes their
own rules in addition to the ones built into this analyzer._

## How to fix violations

Provide one of the above mentioned fixture sources for the fixture data.

## Examples

### Violates

```csharp
using Xunit;

public class Fixture { }

public class xUnit1041
{
    public xUnit1041(Fixture fixture)
    { }

    [Fact]
    public void TestMethod()
    { }
}
```

### Does not violate

#### For v2 and v3

```csharp
using Xunit;

public class Fixture { }

public class xUnit1041 : IClassFixture<Fixture>
{
    public xUnit1041(Fixture fixture)
    { }

    [Fact]
    public void TestMethod()
    { }
}
```

```csharp
using Xunit;

public class Fixture { }

[CollectionDefinition(nameof(MyCollection))]
public class MyCollection : IClassFixture<Fixture> { }

[Collection(nameof(MyCollection))]
public class xUnit1041
{
    public xUnit1041(Fixture fixture)
    { }

    [Fact]
    public void TestMethod()
    { }
}
```

```csharp
using Xunit;

public class Fixture { }

[CollectionDefinition(nameof(MyCollection))]
public class MyCollection : ICollectionFixture<Fixture> { }

[Collection(nameof(MyCollection))]
public class xUnit1041
{
    public xUnit1041(Fixture fixture)
    { }

    [Fact]
    public void TestMethod()
    { }
}
```

#### For v3 only

```csharp
using Xunit;

[assembly: AssemblyFixture(typeof(Fixture))]

public class Fixture { }

public class xUnit1041
{
    public xUnit1041(Fixture fixture)
    { }

    [Fact]
    public void TestMethod()
    { }
}
```
