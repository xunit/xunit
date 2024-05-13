---
title: xUnit1036
description: There is no matching method parameter
category: Usage
severity: Error
---

## Cause

When you use `[MemberData]` to target a method and pass that method arguments, you must provide
the correct number of arguments for the method data function's parameters.

## Reason for rule

Providing too many arguments will cause your test to fail because the method data function
cannot be called.

## How to fix violations

To fix a violation of this rule, either remove the excess data or add additional parameters.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1036
{
    public static TheoryData<int> TestData(int n) =>
        new() { n };

    [Theory]
    [MemberData(nameof(TestData), 42, 2112)]
    public void TestMethod(int _)
    { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1036
{
    public static TheoryData<int> TestData(int n) =>
        new() { n };

    [Theory]
    [MemberData(nameof(TestData), 42)]
    public void TestMethod(int _)
    { }
}
```

```csharp
using Xunit;

public class xUnit1036
{
    public static TheoryData<int> TestData(int n, int d) =>
        new() { n * d };

    [Theory]
    [MemberData(nameof(TestData), 42, 2112)]
    public void TestMethod(int _)
    { }
}
```
