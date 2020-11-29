---
title: xUnit1003
description: Theory methods must have test data
category: Usage
severity: Error
---

## Cause

A Theory method does not have test data.

## Reason for rule

If a Theory method does not have test data, it is never run.

## How to fix violations

- Add a data attribute such as InlineData, MemberData, or ClassData to the test method.
- Change `[Theory]` to `[Fact]` if you want a non-parameterized test.

## Examples

### Violates

```csharp
public class TestClass
{
	[Theory]
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
	[InlineData(5)]
	public void TestMethod(int p1)
	{
		Assert.Equal(2 + 2, p1);
	}
}
```
