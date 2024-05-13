---
title: xUnit1037
description: There are fewer TheoryData type arguments than required by the parameters of the test method
category: Usage
severity: Error
---

## Cause

When you use `TheoryData` with `[MemberData]`, the number of generic types in `TheoryData` must match the
number of parameters in the test method. In this case, you have provided too few types to `TheoryData`.

## Reason for rule

You must provide the correct number of arguments to the test method to run the test.

## How to fix violations

To fix a violation of this rule, either add more type parameters to match the method signature, or remove
parameters from the test method.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1037
{
    public static TheoryData<int> PropertyData =>
        new() { 1, 2, 3 };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(int _1, string _2) { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1037
{
    public static TheoryData<int, string> PropertyData =>
        new() { { 1, "Hello" }, { 2, "World" } };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(int _1, string _2) { }
}
```

```csharp
using Xunit;

public class xUnit1037
{
    public static TheoryData<int> PropertyData =>
        new() { 1, 2, 3 };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(int _) { }
}
```

