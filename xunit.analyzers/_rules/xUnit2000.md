---
title: xUnit2000
description: Constants and literals should be the expected argument
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when the expected argument to `Assert.Equal`, `AssertNotEqual`, `Assert.StrictEqual`, or `Assert.NotStrictEqual` is not the expected value (such as a constant or literal).

## Reason for rule

The expected value in equality assertions should always be the expected argument. This will ensure that generated messages explaining the test failure will correctly match the situation.

## How to fix violations

To fix a violation of this rule, swap the arguments in the assertion, so that the expected value is the first.

## Examples

### Violates

```csharp
[Fact]
public void AdditionExample()
{
	var result = 2 + 3;

	Assert.Equal(result, 5);
}
```

### Does not violate

```csharp
[Fact]
public void AdditionExample()
{
	var result = 2 + 3;

	Assert.Equal(5, result);
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit2000 // Constants and literals should be the expected argument
#pragma warning restore xUnit2000 // Constants and literals should be the expected argument
```
