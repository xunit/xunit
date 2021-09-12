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
Assert.Equals(42, value);
```

```csharp
Assert.ReferenceEquals(expectedObj, actualObj);
```

### Does not violate

```csharp
Assert.Equal(42, value);
```

```csharp
Assert.Same(expectedObj, actualObj);
```
