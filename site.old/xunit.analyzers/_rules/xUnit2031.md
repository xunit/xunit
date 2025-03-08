---
title: xUnit2031
description: Do not use Where clause with Assert.Single
category: Assertions
severity: Warning
v2: true
v3: true
---

## Cause

A violation of this rule occurs when using a LINQ `Where` clause to filter items before calling `Assert.Single`.

## Reason for rule

A more concise overload of `Assert.Single` allows filtering and shows intent better.

## How to fix violations

To fix a violation of this rule, use the overload of `Assert.Single` that takes a filter function.

## Examples

### Violates

```csharp
using System.Linq;
using Xunit;

public class xUnit2031
{
    [Fact]
    public void TestMethod()
    {
        int[] collection = [1, 3, 5, 6, 9];

        Assert.Single(collection.Where(i => i % 2 == 0));
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2031
{
    [Fact]
    public void TestMethod()
    {
        int[] collection = [1, 3, 5, 6, 9];

        Assert.Single(collection, i => i % 2 == 0);
    }
}
```
