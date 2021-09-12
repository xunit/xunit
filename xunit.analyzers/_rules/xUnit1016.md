---
title: xUnit1016
description: MemberData must reference a public member
category: Usage
severity: Error
---

## Cause

This rule is triggered when your `[MemberData]` attribute points to a non-`public` member.

## Reason for rule

`[MemberData]` attributes may only point at `public` members, or else they will fail at runtime.

## How to fix violations

To fix a violation of this rule, make the data member `public` visibility.

## Examples

### Violates

```csharp
public class TestClass
{
	protected static IEnumerable<object[]> TestData;

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
