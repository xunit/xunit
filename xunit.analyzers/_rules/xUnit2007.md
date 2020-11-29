---
title: xUnit2007
description: Do not use typeof expression to check the type
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when the `typeof` operator is used with a type checking assert.

## Reason for rule

When the expected type is known at compile-time, the generic overload should be used.

## How to fix violations

Use the generic overload of `Assert.IsType`, `Assert.IsNotType`, or `Assert.IsAssignableFrom`.

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
	string result = "foo bar baz";

	Assert.IsType(typeof(string), result);
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
	string result = "foo bar baz";

	Assert.IsType<string>(result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2007 // Do not use typeof expression to check the type
#pragma warning restore xUnit2007 // Do not use typeof expression to check the type
```
