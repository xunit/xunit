---
title: xUnit1007
description: ClassData must point at a valid class
category: Usage
severity: Error
---

## Cause

The type referenced by the `[ClassData]` attribute does not implement `IEnumerable<object[]>` or does not have a public parameterless constructor.

## Reason for rule

xUnit.net will attempt to instantiate and enumerate the type specified in `[ClassData]` in order to retrieve test data for the theory. In order for instantiation to succeed, there must be a public parameterless constructor. In order for enumeration to work, the type must implement `IEnumerable<object[]>`.

## How to fix violations

To fix a violation of this rule, make sure that the type specified in the `[ClassData]` attribute meets all of these requirements:

* Is a `class` or a `struct` type.
* Implements `IEnumerable<object[]>`.
* Defines a public parameterless constructor. (The C# and VB.NET compilers will implicitly provide a suitable default constructor if you do not define any constructors at all.)

## Examples

### Violates

```csharp
class TestData
{
}

class Tests
{
	[Theory]
	[ClassData(typeof(TestData))]
	public void TestMethod(int amount, string productType)
	{
	}
}
```

### Does not violate

```csharp
class TestData : IEnumerable<object[]>
{
	public IEnumerator<object[]> GetEnumerator()
	{
		yield return new object[] { 1, "book" };
		yield return new object[] { 1, "magnifying glass" };
	}

	IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
}

class Tests
{
	[Theory]
	[ClassData(typeof(TestData))]
	public void TestMethod(int amount, string productType)
	{
	}
}
```
