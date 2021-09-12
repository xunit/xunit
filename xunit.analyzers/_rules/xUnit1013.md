---
title: xUnit1013
description: Public method should be marked as test
category: Usage
severity: Warning
---

## Cause

This rule is trigger by having a public method in a test class that is not marked as a test.

## Reason for rule

It is frequently oversight to have a public method in a test class which isn't a test method.

## How to fix violations

To fix a violation of this rule, you may:

* Annotate the method with `Fact` or `Theory` attributes
* Change the visibility of the method to something other than `public`

## Examples

### Violates

```csharp
public class Tests
{
	[Fact]
	public void Test1() { }

	public void Test2() { }
}
```

### Does not violate

```csharp
public class Tests
{
	[Fact]
	public void Test1() { }

	[Fact]
	public void Test2() { }
}
```

```csharp
public class Tests
{
	[Fact]
	public void Test1() { }

	internal void Test2() { }
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit1013 // Public method should be marked as test
#pragma warning restore xUnit1013 // Public method should be marked as test
```

## Opt-out for extension authors

Some xUnit.net extensions provide alternative attributes for annotating tests. Such attributes should be annotated with a marker attribute to prevent this rule from firing for valid usages of the extension. The marker attribute has to be called `IgnoreXunitAnalyzersRule1013` (in any or no namespace).

```csharp
public sealed class IgnoreXunitAnalyzersRule1013Attribute : Attribute { }

[IgnoreXunitAnalyzersRule1013]
public class CustomTestTypeAttribute : Attribute { }

public class TestClass
{
	[Fact]
	public void TestMethod() { }

	[CustomTestType]
	public void CustomTestMethod() {}
}
```
