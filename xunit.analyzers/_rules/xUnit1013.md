---
title: xUnit1013
description: Public method should be marked as test
category: Usage
severity: Warning
---

## Cause

A test class (i.e. a class that has methods annotated with `Fact` or `Theory` attributes) has a public method that is not marked as a test.

## Reason for rule

Test classes do not typically need to expose public methods that are not tests. A violation indicates that a possible test method is not properly annotated and will therefore not be executed by the test runner.

## How to fix violations

To fix a violation of this rule, annotate the method with `Fact` or `Theory` attributes.

## Examples

### Violates

```csharp
public class Tests
{
	[Fact]
	public void Test1()
	{
		Helper();
	}

	public void Test2()
	{
		Helper();
	}

	public void Helper() {}
}
```

### Does not violate

```csharp
public class Tests
{
	[Fact]
	public void Test1()
	{
		Helper();
	}

	[Fact]
	public void Test2()
	{
		Helper();
	}

	private void Helper() {}
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
