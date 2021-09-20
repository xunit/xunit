---
title: xUnit2006
description: Do not use invalid string equality check
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when the generic overloads of `Assert.Equal` or `Assert.StrictEqual` are used with `string`.

## Reason for rule

There is an optimized overload of `Assert.Equal` for `string` arguments.

## How to fix violations

To fix a violation of this rule, use the non-generic version of `Assert.Equal` with the `string` overload.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit2006
{
    [Fact]
    public void TestMethod()
    {
        var result = "foo";

        Assert.Equal<string>("foo", result);
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2006
{
    [Fact]
    public void TestMethod()
    {
        var result = "foo";

        Assert.Equal("foo", result);
    }
}
```
