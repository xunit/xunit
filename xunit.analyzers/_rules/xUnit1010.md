---
title: xUnit1010
description: The value is not convertible to the method parameter type
category: Usage
severity: Error
---

## Cause

This rule is triggered by having data in an `[InlineData]` attribute which is not compatible with the parameter type.

## Reason for rule

Mismatched data vs. parameter type results in a runtime error where the test is not able to run.

## How to fix violations

To fix a violation of this rule, change either the data or the parameter type to match.

## Examples

### Violates

```csharp
public class TestClass
{
	[Theory]
	[InlineData("42")]
	public void TestMethod(int value) { }
}
```

### Does not violate

```csharp
public class TestClass
{
	[Theory]
	[InlineData("42")]
	public void TestMethod(string value) { }
}
```

```csharp
public class TestClass
{
	[Theory]
	[InlineData(42)]
	public void TestMethod(int value) { }
}
```
