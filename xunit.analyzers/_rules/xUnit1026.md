---
title: xUnit1026
description: Theory methods should use all of their parameters
category: Usage
severity: Warning
---

## Cause

This rule is triggered by having an unused parameter on your `[Theory]`.

## Reason for rule

Unused parameters are typically an indication of some kind of coding error (typically a parameter that was previously used and is no longer required, or a parameter that was overlooked in the test implementation).

## How to fix violations

To fix a violation of this rule, you may:

* Remove the unused parameter (and all associated data)
* Use the unused parameter

## Examples

### Violates

```csharp
public class TestClass
{
	[Theory]
	[InlineData("Joe", 42)]
	public void ValidateGreeting(string name, int age)
	{
		var result = MyGreetingService.Greet(name);

		Assert.Equal("Hello, Joe!", result);
	}
}
```

### Does not violate

```csharp
public class TestClass
{
	[Theory]
	[InlineData("Joe")]
	public void ValidateGreeting(string name)
	{
		var result = MyGreetingService.Greet(name);

		Assert.Equal("Hello, Joe!", result);
	}
}
```

```csharp
public class TestClass
{
	[Theory]
	[InlineData("Joe", 42)]
	public void ValidateGreeting(string name, int age)
	{
		var result = MyGreetingService.Greet(name, age);

		Assert.Equal("Hello, Joe! How do you like being 42 years old?", result);
	}
}
```

## How to suppress violations

**If the severity of your analyzer isn't _Warning_, delete this section.**

```csharp
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters
#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
```
