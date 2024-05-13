---
title: xUnit1033
description: Test classes decorated with 'Xunit.IClassFixture<TFixture>' or 'Xunit.ICollectionFixture<TFixture>' should add a constructor argument of type TFixture
category: Usage
severity: Info
---

## Cause

When a test class is decorated with a fixture annotation, that fixture instance can be accepted into the test
class's constructor. This analyzer detects when the user may have forgotten to do so.

## Reason for rule

Developers usually want access to the fixture data when running the test. (This is marked as Info level because
it is not a requirement, just a suggestion that the user may have inadvertently forgotten to add the constructor,
as well as guidance to new developers who may not have realized that constructor injection is how fixture
instances as passed to tests.)

## How to fix violations

To fix a violation of this rule, add the fixture item to the constructor.

## Examples

### Violates

```csharp
using Xunit;

public class MyFixture { }

public class xUnit1033 : IClassFixture<MyFixture>
{
    [Fact]
    public void TestMethod() { }
}
```

### Does not violate

```csharp
using Xunit;

public class MyFixture { }

public class xUnit1033 : IClassFixture<MyFixture>
{
    private readonly MyFixture fixture;

    public xUnit1033(MyFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void TestMethod() { }
}
```
