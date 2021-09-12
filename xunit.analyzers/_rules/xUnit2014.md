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
Assert.Throws<DivideByZeroException>(() => MyMath.DivideAsync(1, 0));
```

### Does not violate

```csharp
await Assert.ThrowsAsync<DivideByZeroException>(() => MyMath.DivideAsync(1, 0));
```
