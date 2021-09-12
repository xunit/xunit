---
title: xUnit1018
description: MemberData must reference a valid member kind
category: Usage
severity: Error
---

## Cause

This rule is triggered when your `[MemberData]` attribute points to a member which is not of the supported kind (field, property, or method).

## Reason for rule

`[MemberData]` attributes which do not point at one of the supported member kinds will fail at runtime.

## How to fix violations

To fix a violation of this rule, update the `[MemberData]` to point at a valid member kind.

## Examples

### Violates

```csharp
public class TestClass
{
	public static event EventHandler TestData;

	[Theory]
	[MemberData(nameof(TestData))]
	public void TestMethod(string greeting, int age) { }
}
```

### Does not violate

```csharp
public class TestClass
{
	public static IEnumerable<object[]> TestData;

	[Theory]
	[MemberData(nameof(TestData))]
	public void TestMethod(string greeting, int age) { }
}
```

```csharp
public class TestClass
{
	public static IEnumerable<object[]> TestData { get; set; }

	[Theory]
	[MemberData(nameof(TestData))]
	public void TestMethod(string greeting, int age) { }
}
```

```csharp
public class TestClass
{
	public static IEnumerable<object[]> TestData() { }

	[Theory]
	[MemberData(nameof(TestData))]
	public void TestMethod(string greeting, int age) { }
}
```
