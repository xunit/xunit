---
title: xUnit1024
description: Test methods cannot have overloads
category: Usage
severity: Error
---

## Cause

This rule is triggered when you have more than one public methods with the same name, and at least one of them is marked as a test method.

## Reason for rule

xUnit.net does not support method overloads for test methods. Any test method must have a unique name in the test class.

## How to fix violations

To fix a violation of this rule, you may:

* Rename the extra method(s)
* Delete the extra method(s)
* Mark the extra method(s) with visibility other than `public`

## Examples

### Violates

```csharp
public class TestClass
{
	[Fact]
	public void Method() { }

	public void Method(int age) { }
}
```

### Does not violate

```csharp
public class TestClass
{
	[Fact]
	public void Method() { }

	public void Method2(int age) { }
}
```

```csharp
public class TestClass
{
	[Fact]
	public void Method() { }
}
```

```csharp
public class TestClass
{
	[Fact]
	public void Method() { }

	private void Method(int age) { }
}
```
