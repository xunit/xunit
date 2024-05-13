---
title: xUnit2018
description: Do not compare an object's exact type to an abstract class or interface
category: Assertions
severity: Warning
---

## Cause

This rule is triggered by using `Assert.IsType` or `Assert.IsNotType` with an interface or abstract type.

## Reason for rule

The checks for `Assert.IsType` and `Assert.IsNotType` are an exact type check, which means no value can ever satisfy the test.
You should use `Assert.IsAssignableFrom` or `Assert.IsNotAssignableFrom` instead. (Note: `Assert.IsNotAssignableFrom` was introduced
in xUnit.net v2 2.5, so you may need to upgrade in order to use it.)

## How to fix violations

To fix a violation of this rule, you may:

* Change `Assert.IsType` to `Assert.IsAssignableFrom` and/or `Assert.IsNotType` to `Assert.IsNotAssignableFrom`.
* Convert the check to use a non-interface/abstract type

## Examples

### Violates

```csharp
using System;
using Xunit;

public class xUnit2018
{
    [Fact]
    public void TestMethod()
    {
        var result = new object();

        Assert.IsType<IDisposable>(result);
        Assert.IsNotType<IDisposable>(result);
    }
}
```

### Does not violate

```csharp
using System;
using Xunit;

public class xUnit2018
{
    [Fact]
    public void TestMethod()
    {
        var result = new object();

        Assert.IsAssignableFrom<IDisposable>(result);
        Assert.IsNotAssignableFrom<IDisposable>(result);
    }
}
```

```csharp
using System;
using Xunit;

public class xUnit2018
{
    [Fact]
    public void TestMethod()
    {
        var result = new object();

        Assert.IsType<object>(result);
        Assert.IsNotType<object>(result);
    }
}
```
