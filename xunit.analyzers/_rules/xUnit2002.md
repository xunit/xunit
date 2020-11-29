---
title: xUnit2002
description: Do not use null check on value type
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Assert.Null` or `Assert.NotNull` are used on a value type.

## Reason for rule

Value types cannot be `null`. As such, it does not make sense to compare them to `null`.

## How to fix violations

To fix a violation of this rule, either remove the assertion or change the object’s type to a reference type.

## Examples

### Violates

```csharp
[Fact]
public void ExampleTest()
{
	int result = GetSomeValue();

	Assert.NotNull(result);
	Assert.True(result > 4);
}
```

### Does not violate

```csharp
[Fact]
public void ExampleTest()
{
	int result = GetSomeValue();

	Assert.True(result > 4);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2002 // Do not use null check on value type
#pragma warning restore xUnit2002 // Do not use null check on value type
```
