---
title: xUnit2011
description: Do not use empty collection check
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Assert.Collection` is used without element inspectors to check for an empty collection.

## Reason for rule

There are specialized assertions for checking collection sizes.

## How to fix violations

To fix a violation of this rule, you can:

* Use `Assert.Empty` instead.
* Add element inspectors to the `Assert.Collection` call.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit2011
{
    [Fact]
    public void TestMethod()
    {
        var result = new[] { 1 };

        Assert.Collection(result);
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2011
{
    [Fact]
    public void TestMethod()
    {
        var result = new[] { 1 };

        Assert.Empty(result);
    }
}
```

```csharp
using Xunit;

public class xUnit2011
{
    [Fact]
    public void TestMethod()
    {
        var result = new[] { 1 };

        Assert.Collection(
            result,
            value => Assert.Equal(1, value)
        );
    }
}
```
