---
title: xUnit2010
description: Do not use boolean check to check for string equality
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Assert.True` or `Assert.False` are used with `string.Equals` to check if two strings are equal.

## Reason for rule

`Assert.Equal` or `Assert.Equal` should be used because they give more detailed information upon failure.

## How to fix violations

Replace the assertions with `Assert.Equal` or `Assert.NotEqual`.

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
	string result = "foo bar baz";

	Assert.True(string.Equals("foo bar baz", result));
	Assert.False(string.Equals("hello world", result));
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
	string result = "foo bar baz";

	Assert.Equal("foo bar baz", result);
	Assert.NotEqual("hello world", result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2010 // Do not use boolean check to check for string equality
#pragma warning restore xUnit2010 // Do not use boolean check to check for string equality
```
