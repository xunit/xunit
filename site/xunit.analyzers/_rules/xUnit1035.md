---
title: xUnit1035
description: The value is not convertible to the method parameter type
category: Usage
severity: Error
---

## Cause

When you use `[MemberData]` to target a method and pass that method arguments, the values must match the
type specified in the method parameters.

## Reason for rule

Passing the wrong type of value will result in your test failing because the provided data cannot be
correctly passed to the method data function.

## How to fix violations

To fix a violation of this rule, fix either the data value or the parameter type.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1035
{
    public static TheoryData<string> TestData(string s) =>
        new() { s };

    [Theory]
    [MemberData(nameof(TestData), new object[] { 2112 })]
    public void TestMethod(string _)
    { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1035
{
    public static TheoryData<string> TestData(int n) =>
        new() { n.ToString() };

    [Theory]
    [MemberData(nameof(TestData), new object[] { 2112 })]
    public void TestMethod(string _)
    { }
}
```

```csharp
using Xunit;

public class xUnit1035
{
    public static TheoryData<string> TestData(string s) =>
        new() { s };

    [Theory]
    [MemberData(nameof(TestData), new object[] { "2112" })]
    public void TestMethod(string _)
    { }
}
```
