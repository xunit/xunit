---
title: xUnit2017
description: Do not use Contains() to check if a value exists in a collection
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Enumerable.Contains()` is used to check if a value exists in a collection.

## Reason for rule

There are specialized assertions for checking for elements in collections.

## How to fix violations

Use `Assert.Contains` or `Assert.DoesNotContain` instead.

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
	IEnumerable<string> result = GetItems();

	Assert.True(result.Contains("foo"));
	Assert.False(result.Contains("bar"));
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
	IEnumerable<string> result = GetItems();

	Assert.Contains("foo", result);
	Assert.DoesNotContain("bar", result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2017 // Do not use Contains() to check if a value exists in a collection
#pragma warning restore xUnit2017 // Do not use Contains() to check if a value exists in a collection
```
