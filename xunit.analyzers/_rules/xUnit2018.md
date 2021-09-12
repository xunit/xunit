---
title: xUnit2018
description: Do not compare an object's exact type to an abstract class or interface
category: Assertions
severity: Warning
---

## Cause

This rule is triggered by using `Assert.IsType` with an interface or abstract type.

## Reason for rule

The check for `Assert.IsType` is an exact type check, which means no value can ever satisfy the test.

## How to fix violations

To fix a violation of this rule, you may:

* Change `Assert.IsType` to `Assert.IsAssignableFrom`
* Convert the check to use a non-interface/abstract type

## Examples

### Violates

```csharp
Assert.IsType<IDisposable>(myObject);
```

### Does not violate

```csharp
Assert.IsAssignableFrom<IDisposable>(myObject);
```

```csharp
Assert.IsType<MyConcreteType>(myObject);
```

## How to suppress violations

**If the severity of your analyzer isn't _Warning_, delete this section.**

```csharp
#pragma warning disable xUnit2018 // Do not compare an object's exact type to an abstract class or interface
#pragma warning restore xUnit2018 // Do not compare an object's exact type to an abstract class or interface
```
