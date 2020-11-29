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
Assert.Equals(expectedDouble, actualDouble, 16);
Assert.Equals(expectedDouble, actualDouble, int.MaxValue);
Assert.Equals(expectedDecimal, actualDecimal, 32);
```

### Does not violate

```csharp
Assert.Equals(expectedDouble, actualDouble, 0);
Assert.Equals(expectedDouble, actualDouble, 15);
Assert.Equals(expectedDecimal, actualDecimal, 0);
Assert.Equals(expectedDecimal, actualDecimal, 28);
```