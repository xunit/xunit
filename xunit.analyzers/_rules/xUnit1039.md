---
title: xUnit1039
description: The type argument to TheoryData is not compatible with the type of the corresponding test method parameter
category: Usage
severity: Error
---

## Cause

The type argument given to `TheoryData` is not compatible with the matching parameter in the test method.

## Reason for rule

When the data types aren't compatible, then the test will fail at runtime with a type mismatch, instead of
running the test.

## How to fix violations

To fix a violation of this rule, make the types in the parameter and `TheoryData` compatible.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1039
{
    public static TheoryData<int> PropertyData =>
        new() { 1, 2, 3 };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string _) { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1039
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

public class xUnit1039
{
    public static TheoryData<string> PropertyData =>
        new() { "1", "2", "3" };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string _) { }
}
```
