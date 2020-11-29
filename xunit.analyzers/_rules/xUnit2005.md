---
title: xUnit2005
description: Do not use identity check on value type
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when two value type objects are compared using `Assert.Same` or `Assert.NotSame`.

## Reason for rule

`Assert.Same` and `Assert.NotSame` both use [`Object.ReferenceEquals`](https://msdn.microsoft.com/en-us/library/system.object.referenceequals.aspx) to compare objects. This always fails for value types since the values will be boxed before they are passed to the method, creating two different references.

## How to fix violations

To fix a violation of this rule, use `Assert.Equal` or `Assert.NotEqual` instead.

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
	DateTime result = GetDateResult();

	Assert.Same(new DateTime(2017, 01, 01), result);
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
	DateTime result = GetDateResult();

	Assert.Equal(new DateTime(2017, 01, 01), result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2005 // Do not use identity check on value type
#pragma warning restore xUnit2005 // Do not use identity check on value type
```
