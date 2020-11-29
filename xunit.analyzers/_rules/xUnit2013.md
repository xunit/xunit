---
title: xUnit2013
description: Do not use equality check to check for collection size.
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Assert.Equals` or `Assert.NotEquals` are used to check if a collection has 0 or 1 elements.

## Reason for rule

There are specialized assertions for checking collection sizes.

## How to fix violations

Use `Assert.Empty`, `Assert.NotEmpty`, or `Assert.Single` instead.

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
	IEnumerable<string> result = GetItems();

	Assert.Equal(1, result.Count());
	Assert.Equal(0, result.Count());
	Assert.NotEqual(0, result.Count());
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
	IEnumerable<string> result = GetItems();

	Assert.Single(result);
	Assert.Empty(result);
	Assert.NotEmpty(result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.
#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.
```
