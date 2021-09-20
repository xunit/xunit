---
title: xUnit2012
description: Do not use Enumerable.Any() to check if a value exists in a collection
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Enumerable.Any` is used to check if a value matching a predicate exists in a collection.

## Reason for rule

There are specialized assertions for checking for elements in collections.

## How to fix violations

Replace `Assert.True` with `Assert.Contains` and/or `Assert.False` with `Assert.DoesNotContain`.

## Examples

### Violates

```csharp
using System.Linq;
using Xunit;

public class xUnit2012
{
    [Fact]
    public void TestMethod()
    {
        var result = new[] { "Hello" };

        Assert.True(result.Any(value => value.Length == 5));
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2012
{
    [Fact]
    public void TestMethod()
    {
        var result = new[] { "Hello" };

        Assert.Contains(result, value => value.Length == 5);
    }
}
```
