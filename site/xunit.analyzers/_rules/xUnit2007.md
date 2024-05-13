---
title: xUnit2007
description: Do not use typeof expression to check the type
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when the `typeof` operator is used with a type checking assert.

## Reason for rule

When the expected type is known at compile-time, the generic overload should be used. In addition to being more concise, it also returns the value cast to the appropriate type when the assert succeeds, for use in later assertions.

## How to fix violations

Use the generic overload of `Assert.IsType`, `Assert.IsNotType`, or `Assert.IsAssignableFrom`.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit2007
{
    [Fact]
    public void TestMethod()
    {
        var result = "foo";

        Assert.IsType(typeof(string), result);
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2007
{
    [Fact]
    public void TestMethod()
    {
        var result = "foo";

        Assert.IsType<string>(result);
    }
}
```
