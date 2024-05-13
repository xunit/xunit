---
title: xUnit2001
description: Do not use invalid equality check
category: Assertions
severity: Hidden
---

## Cause

`Assert.Equals` or `Assert.ReferenceEquals` is used.

## Reason for rule

`Assert.Equals` does not assert that two objects are equal; it exists only to hide the static `Equals` method inherited from `object`. It's a similar story for `Assert.ReferenceEquals`.

## How to fix violations

To fix a violation of this rule, use `Assert.Equal` instead of `Equals` and `Assert.Same` instead of `ReferenceEquals`.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit2001
{
    [Fact]
    public void TestMethod()
    {
        var result = 21 * 2;

        Assert.Equals(42, result);
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2001
{
    [Fact]
    public void TestMethod()
    {
        var result = 21 * 2;

        Assert.Equal(42, result);
    }
}
```
