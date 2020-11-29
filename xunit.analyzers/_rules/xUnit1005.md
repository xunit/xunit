---
title: xUnit1005
description: Fact methods should not have test data
category: Usage
severity: Warning
---

## Cause

A fact method has one or more attributes that provide test data.

## Reason for rule

Unlike theory methods, fact methods do not have any parameters. Providing a fact method with test data is therefore pointless, as there is no way to actually pass that data to the test method.

## How to fix violations

To fix a violation of this rule, either:

* Turn the fact method into a theory method by replacing `[Fact]` with `[Theory]`, if your test method is parameterized.

* Remove any attributes that provide test data, if your test method is not parameterized. (That is, remove all attributes of a type deriving from `DataAttribute`, such as `[ClassData]`, `[InlineData]`, or `[MemberData]`.)

## Examples

### Violates

```csharp
public class TestClass
{
	[Fact, InlineData(1)]
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

	[Theory, InlineData(1)]
	public void ParameterizedTestMethod(int arg)
	{
	}
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit1005 // Fact methods should not have test data
#pragma warning restore xUnit1005 // Fact methods should not have test data
```
