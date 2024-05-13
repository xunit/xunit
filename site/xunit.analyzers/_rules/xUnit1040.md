---
title: xUnit1040
description: The type argument to TheoryData is nullable, while the type of the corresponding test method parameter is not
category: Usage
severity: Warning
---

## Cause

The `TheoryData` type argument is marked as nullable, and the test method argument is marked as non-nullable.

## Reason for rule

Passing `null` data to a test method that isn't expecting it could cause runtime errors or unpredictable test results
(either false positives or false negatives).

## How to fix violations

To fix a violation of this rule, either make the `TheoryData` type non-nullable, or make the test method parameter nullable.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1040
{
    public static TheoryData<string?> PropertyData =>
        new() { "Hello", "World", null };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string _) { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1040
{
    public static TheoryData<string> PropertyData =>
        new() { "Hello", "World" };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string _) { }
}
```

```csharp
using Xunit;

public class xUnit1040
{
    public static TheoryData<string?> PropertyData =>
        new() { "Hello", "World", null };

    [Theory]
    [MemberData(nameof(PropertyData))]
    public void TestMethod(string? _) { }
}
```
