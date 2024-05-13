---
title: xUnit2003
description: Do not use equality check to test for null value
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Assert.Equal`, `AssertNotEqual`, `Assert.StrictEqual`, or `Assert.NotStrictEqual` are used with `null`.

## Reason for rule

`Assert.Null` and `Assert.NotNull` should be used when checking against `null`.

## How to fix violations

To fix a violation of this rule, replace the offending asserts with `Assert.Null` or `Assert.NotNull`.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit2003
{
    [Fact]
    public void TestMethod()
    {
        var result = "Hello world!";

        Assert.Equal(null, result);
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2003
{
    [Fact]
    public void TestMethod()
    {
        var result = "Hello world!";

        Assert.Null(result);
    }
}
```
