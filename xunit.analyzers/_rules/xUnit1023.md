---
title: xUnit1023
description: Theory methods cannot have default parameter values
category: Usage
severity: Error
---

## Cause

One or more parameters of a theory method have default values, and you are using a version of xUnit.net &lt; 2.2.0.

## Reason for rule

Theory methods receive their test data from data attributes such as `[InlineData]`. xUnit.net versions &lt; 2.2.0 require that data attributes provide values for *all* parameters of a theory method; any fallback default parameter values you provide would never be used. Providing default values indicates that you are making an assumption that will never be met in practice.

## How to fix violations

To fix a violation of this rule, either:

* Upgrade xUnit.net to version 2.2.0 or later. These versions have support for optional parameters with theory methods.

* Remove all default values from the theory method's parameter list.

## Examples

### Violates

```csharp
class TestClass
{
	[Theory]
	[InlineData(1)]
	public void TestMethod(int arg = 0)
	{
	}
}
```

### Does not violate

```csharp
class TestClass
{
	[Theory]
	[InlineData(1)]
	public void TestMethod(int arg)
	{
	}
}
```
