---
title: xUnit2020
description: Do not use always-failing boolean assertion to fail a test
category: Assertions
severity: Warning
---

## Cause

This rule is triggered by using `Assert.True(false, "message")` or `Assert.False(true, "message")`.

## Reason for rule

xUnit.net v2 2.5 introduced `Assert.Fail("message")` for this purpose. If you are using v2 2.5 or later, this rule
will trigger (if you're using an older version, it will not trigger).

## How to fix violations

To fix a violation of this rule, you should use `Assert.Fail`.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit2020
{
    [Fact]
    public void TestMethod()
    {
        Assert.True(false, "Failure message");
    }
}
```

```csharp
using Xunit;

public class xUnit2020
{
    [Fact]
    public void TestMethod()
    {
        Assert.False(true, "Failure message");
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2020
{
    [Fact]
    public void TestMethod()
    {
        Assert.Fail("Failure message");
    }
}
```
