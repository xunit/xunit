---
title: xUnit2017
description: Do not use Contains() to check if a value exists in a collection
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Enumerable.Contains()` is used to check if a value exists in a collection.

## Reason for rule

There are specialized assertions for checking for elements in collections.

## How to fix violations

Use `Assert.Contains` or `Assert.DoesNotContain` instead.

## Examples

### Violates

```csharp
using System.Linq;
using Xunit;

public class xUnit2017
{
    [Fact]
    public void TestMethod()
    {
        var result = new[] { "foo", "bar" };

        Assert.True(result.Contains("foo"));
    }
}
```


### Does not violate

```csharp
using Xunit;

public class xUnit2017
{
    [Fact]
    public void TestMethod()
    {
        var result = new[] { "foo", "bar" };

        Assert.Contains("foo", result);
    }
}
```
