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
using System.Text.RegularExpressions;
using Xunit;

public class xUnit2008
{
    [Fact]
    public void TestMethod()
    {
        var result = "foo bar baz";

        Assert.True(Regex.IsMatch(result, "foo (.*?) baz"));
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2008
{
    [Fact]
    public void TestMethod()
    {
        var result = "foo bar baz";

        Assert.Matches("foo (.*?) baz", result);
    }
}
```
