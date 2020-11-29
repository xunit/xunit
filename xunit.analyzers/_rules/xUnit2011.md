---
title: xUnit2011
description: Do not use empty collection check
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Assert.Collection` is used without element inspectors to check for an empty collection.

## Reason for rule

There are specialized assertions for checking collection sizes.

## How to fix violations

Use `Assert.Empty` instead.

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
	IEnumerable<string> result = GetItems();

	Assert.Collection(result);
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
	IEnumerable<string> result = GetItems();

	Assert.Empty(result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2011 // Do not use empty collection check
#pragma warning restore xUnit2011 // Do not use empty collection check
```
