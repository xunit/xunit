---
title: xUnit1000
description: Test classes must be public
category: Usage
severity: Error
---

## Cause

A class containing test methods is not public.

## Reason for rule

xUnit.net will not run the test methods in a class if the class is not public.

## How to fix violations

To fix a violation of this rule, make the test class public.

## Examples

### Violates

```csharp
class TestClass
{
	[Fact]
	public void TestMethod()
	{
	}
}
```

### Does not violate

```csharp
public class TestClass
{
	[Fact]
	public void TestMethod()
	{
	}
}
```
