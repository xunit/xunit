---
title: xUnit1002
description: Test methods cannot have multiple Fact or Theory attributes
category: Usage
severity: Error
---

## Cause

A test method has multiple Fact or Theory attributes.

## Reason for rule

A test method only needs one Fact or Theory attribute.

## How to fix violations

To fix a violation of this rule, remove all but one of the Fact or Theory attributes.

## Examples

### Violates

```csharp
public class TestClass
{
	[Fact, Theory]
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

```csharp
public class TestClass
{
	[Theory]
	public void TestMethod()
	{
	}
}
```
