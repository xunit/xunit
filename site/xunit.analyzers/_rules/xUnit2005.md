---
title: xUnit2005
description: Do not use identity check on value type
category: Assertions
severity: Warning
---

## Cause

A violation of this rule occurs when two value type objects are compared using `Assert.Same` or `Assert.NotSame`.

## Reason for rule

`Assert.Same` and `Assert.NotSame` both use [`Object.ReferenceEquals`](https://msdn.microsoft.com/en-us/library/system.object.referenceequals.aspx) to compare objects. This always fails for value types since the values will be boxed before they are passed to the method, creating two different references (even if the values are the equal).

## How to fix violations

To fix a violation of this rule, use `Assert.Equal` or `Assert.NotEqual` instead.

## Examples

### Violates

```csharp
using System;
using Xunit;

public class xUnit2005
{
    [Fact]
    public void TestMethod()
    {
        var result = DateTime.Now;

        Assert.Same(new DateTime(2017, 1, 1), result);
    }
}
```

### Does not violate

```csharp
using System;
using Xunit;

public class xUnit2005
{
    [Fact]
    public void TestMethod()
    {
        var result = DateTime.Now;

        Assert.Equal(new DateTime(2017, 1, 1), result);
    }
}
```
