---
title: xUnit2032
description: Type assertions based on 'assignable from' are confusingly named
category: Assertions
severity: Info
v2: true
v3: true
---

## Cause

A violation of this rule occurs when using `Assert.IsAssignableFrom` (or `Assert.IsNotAssignableFrom`).

## Reason for rule

There is confusing about which arguments is "from" and when is "to" when using `Assert.IsAssignableFrom` (and `Assert.IsNotAssignableFrom`). We have added an overload of `Assert.IsType` (and `Assert.IsNotType`) that allows users to pass a flag whether they want strict ("exact") type matching behavior (like `Assert.IsType` without the flag) or compatible ("inexact") type matching behavior (like `Assert.IsAssignableFrom`).

## How to fix violations

To fix a violation of this rule, convert the assertion to `Assert.IsType` (or `Assert.IsNotType`) using `exactMatch: false`.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit2032
{
    [Fact]
    public void TestMethod()
    {
        Assert.IsAssignableFrom<object>("Hello world");
    }
}
```

```csharp
using Xunit;

public class xUnit2032
{
    [Fact]
    public void TestMethod()
    {
        Assert.IsNotAssignableFrom<object>("Hello world");
    }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit2032
{
    [Fact]
    public void TestMethod()
    {
        Assert.IsType<object>("Hello world", exactMatch: false);
    }
}
```

```csharp
using Xunit;

public class xUnit2032
{
    [Fact]
    public void TestMethod()
    {
        Assert.IsNotType<object>("Hello world", exactMatch: false);
    }
}
```
