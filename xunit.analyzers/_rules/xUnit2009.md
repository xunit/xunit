---
title: xUnit2009
description: Do not use boolean check to check for substrings
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when `Assert.True` or `Assert.False` are used to check for substrings with string methods like `string.Contains`, `string.StartsWith` and `string.EndsWith`.

## Reason for rule

There are specialized assertions for substring checks.

## How to fix violations

To fix a violation of this rule, replace the offending assertion according to this:

- `Assert.True(str.Contains(word))` => `Assert.Contains(word, str)`
- `Assert.False(str.Contains(word))` => `Assert.DoesNotContain(word, str)`
- `Assert.True(str.StartsWith(word))` => `Assert.StartsWith(word, str)`
- `Assert.True(str.EndsWith(word))` => `Assert.EndsWith(word, str)`

## Examples

### Violates

```csharp
[Fact]
public void ExampleMethod()
{
	string result = "foo bar baz";

	Assert.True(result.Contains("bar"));
	Assert.True(result.StartsWith("foo"));
}
```

### Does not violate

```csharp
[Fact]
public void ExampleMethod()
{
	string result = "foo bar baz";

	Assert.Contains("bar", result);
	Assert.StartsWith("foo", result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2009 // Do not use boolean check to check for substrings
#pragma warning restore xUnit2009 // Do not use boolean check to check for substrings
```
