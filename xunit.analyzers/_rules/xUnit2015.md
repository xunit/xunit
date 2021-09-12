---
title: xUnit2015
description: Do not use typeof expression to check the exception type
category: Assertions
severity: Warning
---

## Cause

This rule is triggered when using the non-generic version of `Assert.Throws` along with a `typeof` expression.

## Reason for rule

When the expected type is known at compile-time, the generic overload should be used. In addition to being more concise, it also returns the exception cast to the appropriate type when the assert succeeds, for use in later assertions.

## How to fix violations

To fix a violation of this rule, use the generic version of `Assert.Throws`.

## Examples

### Violates

Example(s) of code that violates the rule.

```csharp
Assert.Throws(typeof(InvalidOperationException), () => FunctionThatThrows());
```

### Does not violate

Example(s) of code that does not violate the rule.

```csharp
Assert.Throws<InvalidOperationException>(() => FunctionThatThrows());
```

## How to suppress violations

**If the severity of your analyzer isn't _Warning_, delete this section.**

```csharp
#pragma warning disable xUnit2015 // Do not use typeof expression to check the exception type
#pragma warning restore xUnit2015 // Do not use typeof expression to check the exception type
```
