---
title: xUnit1001
description: Fact methods cannot have parameters
category: Usage
severity: Error
---

## Cause

A fact method has one or more parameters.

## Reason for rule

A fact method is a non-parameterized test. xUnit.net will raise a runtime error if it sees a fact method with a non-empty parameter list.

## How to fix violations

To fix a violation of this rule, remove the parameters from the fact method. Alternatively, change the `[Fact]` attribute to `[Theory]`.

## Examples

### Violates

```csharp
public class TestClass
{
	[Fact]
	public void TestMethod(int p1)
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

```csharp
public class TestClass
{
	[Theory]
	public void TestMethod(int p1)
	{
	}
}
```
