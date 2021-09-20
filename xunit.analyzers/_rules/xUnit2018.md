---
title: xUnit2018
description: Do not compare an object's exact type to an abstract class or interface
category: Assertions
severity: Warning
---

## Cause

This rule is triggered by using `Assert.IsType` with an interface or abstract type.

## Reason for rule

The check for `Assert.IsType` is an exact type check, which means no value can ever satisfy the test.

## How to fix violations

To fix a violation of this rule, you may:

* Change `Assert.IsType` to `Assert.IsAssignableFrom`
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
    }
}
```
