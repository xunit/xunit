---
title: xUnit2025
description: The boolean assertion statement can be simplified
category: Assertions
severity: Info
---

## Cause

Boolean assertions which compare with equality against `true` or `false` can be simplified.

## Reason for rule

Simplifying an expression like `Assert.True(x == true)` to just `Assert.True(x)` makes the code simpler. Additionally,
inversions like `Assert.True(x == false)` are much easier to read and understand as `Assert.False(x)`.

## How to fix violations

To fix a violation of this rule, remove the equality test (and update the assertion method, if necessary).

## Examples

### Violates

```csharp
using Xunit;

public class xUnit2025
{
    [Fact]
    public void TestMethod()
    {
        var x = true;

        Assert.True(x == true);
    }
}
```

```csharp
using Xunit;

public class xUnit2025
{
    [Fact]
    public void TestMethod()
    {
        var x = false;

        Assert.True(x != true);
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2025
{
    [Fact]
    public void TestMethod()
    {
        var x = true;

        Assert.True(x);
    }
}
```

```csharp
using Xunit;

public class xUnit2025
{
    [Fact]
    public void TestMethod()
    {
        var x = false;

        Assert.False(x);
    }
}
```
