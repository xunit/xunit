---
title: xUnit2008
description: Do not use boolean check to match on regular expressions
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Assert.True` or `Assert.False` are used to check for regular expression matches.

## Reason for rule

There are specialized assertions for regular expression matching that give more detailed information upon failure.

## How to fix violations

Replace the assertions with `Assert.Matches` or `Assert.DoesNotMatch`.

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
	string result = "foo bar baz";

	Assert.True(Regex.IsMatch(result, "foo (.*?) baz"));
	Assert.False(Regex.IsMatch(result, "hello (.*?)"));
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
	string result = "foo bar baz";

	Assert.Matches("foo (.*?) baz", result);
	Assert.DoesNotMatch("hello (.*?)", result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2008 // Do not use boolean check to match on regular expressions
#pragma warning restore xUnit2008 // Do not use boolean check to match on regular expressions
```
