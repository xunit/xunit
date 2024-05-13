---
title: xUnit2010
description: Do not use boolean check to check for string equality
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Assert.True` or `Assert.False` are used with `string.Equals` to check if two strings are equal.

## Reason for rule

`Assert.Equal` or `Assert.Equal` should be used because they give more detailed information upon failure.

## How to fix violations

Replace `Assert.True` with `Assert.Equal` and/or `Assert.False` with `Assert.NotEqual`.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit2010
{
    [Fact]
    public void TestMethod()
    {
        var result = "foo bar baz";

        Assert.True(string.Equals("foo bar baz", result));
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2010
{
    [Fact]
    public void TestMethod()
    {
        var result = "foo bar baz";

        Assert.Equal("foo bar baz", result);
    }
}
```
