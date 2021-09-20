---
title: xUnit2002
description: Do not use null check on value type
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Assert.Null` or `Assert.NotNull` are used on a value type.

## Reason for rule

Value types cannot be `null`. As such, it does not make sense to compare them to `null`.

## How to fix violations

To fix a violation of this rule, either remove the assertion or change the object’s type to a reference type.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit2002
{
    [Fact]
    public void TestMethod()
    {
        var result = 2 + 3;

        Assert.NotNull(result);
        Assert.True(result > 4);
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2002
{
    [Fact]
    public void TestMethod()
    {
        var result = 2 + 3;

        Assert.True(result > 4);
    }
}
```
