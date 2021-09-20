---
title: xUnit2013
description: Do not use equality check to check for collection size.
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Assert.Equals` or `Assert.NotEquals` are used to check if a collection has 0 or 1 elements.

## Reason for rule

There are specialized assertions for checking collection sizes.

## How to fix violations

Use `Assert.Empty`, `Assert.NotEmpty`, or `Assert.Single` instead.

## Examples

### Violates

```csharp
using System.Linq;
using Xunit;

public class xUnit2013
{
    [Fact]
    public void TestMethod()
    {
        var result = new[] { "Hello" };

        Assert.Equal(1, result.Count());
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2013
{
    [Fact]
    public void TestMethod()
    {
        var result = new[] { "Hello" };

        Assert.Single(result);
    }
}
```
