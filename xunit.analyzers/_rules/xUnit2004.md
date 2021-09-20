---
title: xUnit2004
description: Do not use equality check to test for boolean conditions
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when:

- `Assert.Equal`, `Assert.NotEqual`, `Assert.StrictEqual`, or `Assert.NotStrictEqual` is used
- The expected value is a `true` or `false` literal

## Reason for rule

It's more readable to use `Assert.True` or `Assert.False` instead.

## How to fix violations

For `Equal` and `StrictEqual`

- `Assert.Equal(true, b)` => `Assert.True(b)`
- `Assert.StrictEqual(true, b)` => `Assert.True(b)`
- `Assert.Equal(false, b)` => `Assert.False(b)`
- `Assert.StrictEqual(false, b)` => `Assert.False(b)`

For `NotEqual` and `NotStrictEqual`

- `Assert.NotEqual(true, b)` => `Assert.False(b)`
- `Assert.NotStrictEqual(true, b)` => `Assert.False(b)`
- `Assert.NotEqual(false, b)` => `Assert.True(b)`
- `Assert.NotStrictEqual(false, b)` => `Assert.True(b)`

## Examples

### Violates

```csharp
using Xunit;

public class xUnit2004
{
    [Fact]
    public void TestMethod()
    {
        var result = 2 + 2;

        Assert.Equal(true, result > 3);
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2004
{
    [Fact]
    public void TestMethod()
    {
        var result = 2 + 2;

        Assert.True(result > 3);
    }
}
```
