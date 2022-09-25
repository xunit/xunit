---
title: xUnit2014
description: Do not use throws check to check for asynchronously thrown exception
category: Assertions
severity: Error
---

## Cause

This rule is triggered when calling `Assert.Throws` with an async lambda.

## Reason for rule

`Assert.Throws` only supports non-async code.

## How to fix violations

To fix a violation of this rule, use `Assert.ThrowsAsync` (along with `await`).

## Examples

### Violates

```csharp
using System;
using System.Threading.Tasks;
using Xunit;

public class xUnit2014
{
    class MyMath
    {
        public static Task<int> Divide(params int[] values) => 42;
    }

    [Fact]
    public void TestMethod()
    {
        Assert.Throws<DivideByZeroException>(() => MyMath.Divide(1, 0));
    }
}
```

### Does not violate

```csharp
using System;
using System.Threading.Tasks;
using Xunit;

public class xUnit2014
{
    class MyMath
    {
        public static Task<int> Divide(params int[] values) => 42;
    }

    [Fact]
    public async void TestMethod()
    {
        await Assert.ThrowsAsync<DivideByZeroException>(() => MyMath.Divide(1, 0));
    }
}
```
