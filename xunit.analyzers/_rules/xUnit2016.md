---
title: xUnit2016
description: Keep precision in the allowed range when asserting equality of doubles or decimals.
category: Assertions
severity: Error
---

## Cause

Asserting on equality of two double or decimal values was declared with precision out of the acceptable range.

## Reason for rule

`Assert.Equals` uses `System.Math.Round` internally which imposes limits on the precision parameter of [0..15] for
doubles and [0..28] for decimals.

## How to fix violations

Keep the precision in [0..15] for doubles and [0..28] for decimals.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit2016
{
    [Fact]
    public void TestMethod()
    {
        var actual = 1.1;

        Assert.Equal(1.1, actual, 16);
    }
}

```

### Does not violate

```csharp
using Xunit;

public class xUnit2016
{
    [Fact]
    public void TestMethod()
    {
        var actual = 1.1;

        Assert.Equal(1.1, actual, 15);
    }
}

```
