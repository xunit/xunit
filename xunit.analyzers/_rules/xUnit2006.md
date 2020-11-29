---
title: xUnit2006
description: Do not use invalid string equality check
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when the generic overloads of `Assert.Equal` or `Assert.StrictEqual` are used with `string`.

## Reason for rule

There is an optimized overload of both `Assert.Equal` and `Assert.StrictEqual` for `string` arguments.

## How to fix violations

To fix a violation of this rule, remove the generic argument to use the `string` overload.

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
	string result = "foo";

	Assert.Equal<string>("foo", result);
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
	string result = "foo";

	Assert.Equal("foo", result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2006 // Do not use invalid string equality check
#pragma warning restore xUnit2006 // Do not use invalid string equality check
```
