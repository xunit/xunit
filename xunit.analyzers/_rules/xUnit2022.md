---
title: xUnit2022
description: Boolean assertions should not be negated
category: Assertions
severity: Info
---

## Cause

This rule is triggered when you call a boolean assertion with a negated expression.

## Reason for rule

The message that results from a negated expression is often less clear than the one that would
result from a positive expression.

## How to fix violations

To fix a violation of this rule, remove the negation and invert the assertion.

## Examples

### Violates

```csharp
using Xunit;

public class TestClass
{
    [Fact]
    public void TestMethod()
    {
        Assert.True(!condition);
    }
}
```

### Does not violate

```csharp
using Xunit;

public class TestClass
{
    [Fact]
    public void TestMethod()
    {
        Assert.False(condition);
    }
}
```
