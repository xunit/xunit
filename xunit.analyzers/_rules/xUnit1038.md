---
title: xUnit1038
description: There are more TheoryData type arguments than allowed by the parameters of the test method
category: Usage
severity: Error
---

## Cause

When you use `TheoryData` with `[MemberData]`, the number of generic types in `TheoryData` must match the
number of parameters in the test method. In this case, you have provided too many types to `TheoryData`.

## Reason for rule

You must provide the correct number of arguments to the test method to run the test.

## How to fix violations

To fix a violation of this rule, either remove unused type arguments, or add more parameters.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1038
{
    public static TheoryData<int, string> PropertyData =>
        new() { { 1, "Hello" }, { 2, "World" } };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(int _) { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1038
{
    public static TheoryData<int> PropertyData =>
        new() { 1, 2, 3 };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(int _) { }
}
```

```csharp
using Xunit;

public class xUnit1038
{
    public static TheoryData<int, string> PropertyData =>
        new() { { 1, "Hello" }, { 2, "World" } };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(int _1, string _2) { }
}
```
