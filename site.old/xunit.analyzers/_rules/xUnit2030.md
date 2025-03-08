---
title: xUnit2030
description: Do not use Assert.NotEmpty to check if a value exists in a collection
category: Assertions
severity: Warning
v2: true
v3: true
---

## Cause

This issue is triggered by using a LINQ `Where` class with `Assert.NotEmpty` to make sure matching items exist
in a collection.

## Reason for rule

Using a LINQ `Where` clause to filter items from a collection, and then using `Assert.NotEmpty` to make sure there
are matching items, gives a less useful reporting result than using `Assert.Contains` (as well as the updated
code better illustrating the intent of the test).

## How to fix violations

To fix a violation of this rule, it is recommended that you use `Assert.Contains` instead.

## Examples

### Violates

```csharp
using System.Linq;
using Xunit;

public class xUnit2030
{
    [Fact]
    public void TestMethod()
    {
        var collection = new[] { 1, 2, 3 };

        Assert.NotEmpty(collection.Where(i => i % 2 == 0));
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2030
{
    [Fact]
    public void TestMethod()
    {
        var collection = new[] { 1, 2, 3 };

        Assert.Contains(collection, i => i % 2 == 0);
    }
}
```
