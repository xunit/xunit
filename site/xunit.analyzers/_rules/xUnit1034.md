---
title: xUnit1034
description: Null should only be used for nullable parameters
category: Usage
severity: Warning
---

## Cause

When you use `[MemberData]` to target a method and pass that method arguments, the values must match
the nullability of the method parameters (that is, `null` values are not allowed to be passed to
non-nullable parameters of the method data function).

## Reason for rule

Passing `null` values to functions that aren't expecting them can be cause for runtime errors.

## How to fix violations

To fix a violation of this rule, either replace the `null` argument with a non-`null` value, or
update the parameter to accept a nullable value.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1034
{
    public static TheoryData<string> TestData(int n, string s) =>
        new() { $"Hello {s}! x {n}" };

    [Theory]
    [MemberData(nameof(TestData), 2112, null)]
    public void TestMethod(string _)
    { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1034
{
    public static TheoryData<string> TestData(int n, string s) =>
        new() { $"Hello {s}! x {n}" };

    [Theory]
    [MemberData(nameof(TestData), 2112, "Brad")]
    public void TestMethod(string _)
    { }
}
```

```csharp
using Xunit;

public class xUnit1034
{
    public static TheoryData<string> TestData(int n, string? s) =>
        new() { $"Hello {s ?? "friend"}! x {n}" };

    [Theory]
    [MemberData(nameof(TestData), 2112, null)]
    public void TestMethod(string _)
    { }
}
```
