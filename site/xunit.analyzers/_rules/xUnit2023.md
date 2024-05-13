---
title: xUnit2023
description: Do not use collection methods for single-item collections
category: Assertions
severity: Info
---

## Cause

A violation of this rule occurs when you use `Assert.Collection` to verify that a collection
has a single item in it.

## Reason for rule

Using `Assert.Collection` is designed for inspecting multiple items in a collection. When a
collection is only expected to have a single item in it, using `Assert.Single` to get the item
makes the code more concise and readable, especially when performing multiple assertions against
the item.

## How to fix violations

To fix a violation of this rule, replace `Assert.Collection` with `Assert.Single` and move the
assertions out into the body of the unit test.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit2023
{
    [Fact]
    public void TestMethod()
    {
        var collection = new[] { 1 };

        Assert.Collection(
          collection,
          item => Assert.Equal(1, item)
        );
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2023
{
    [Fact]
    public void TestMethod()
    {
        var collection = new[] { 1 };

        var item = Assert.Single(collection);
        Assert.Equal(1, item);
    }
}
```
