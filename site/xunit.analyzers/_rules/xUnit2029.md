---
title: xUnit2029
description: Do not use Assert.Empty to check if a value does not exist in a collection
category: Assertions
severity: Warning
v2: true
v3: true
---

## Cause

This issue is triggered by using a LINQ `Where` class with `Assert.Empty` to make sure no matching items exist
in a collection.

## Reason for rule

Using a LINQ `Where` clause to filter items from a collection, and then using `Assert.Empty` to make sure there are
no matching items, gives a less useful reporting result than using `Assert.DoesNotContain` (as well as the updated
code better illustrating the intent of the test).

## How to fix violations

To fix a violation of this rule, it is recommended that you use `Assert.DoesNotContain` instead.

## Examples

### Violates

```csharp
using System.Linq;
using Xunit;

public class xUnit2029
{
    [Fact]
    public void TestMethod()
    {
        var collection = new[] { 1, 2, 3 };

        Assert.Empty(collection.Where(i => i % 2 == 0));
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2029
{
    [Fact]
    public void TestMethod()
    {
        var collection = new[] { 1, 2, 3 };

        Assert.DoesNotContain(collection, i => i % 2 == 0);
    }
}
```
