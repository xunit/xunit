---
title: xUnit1044
description: Avoid using TheoryData type arguments that are not serializable
category: Usage
severity: Info
v2: true
v3: true
---

## Cause

A violation of this rule occurs when a type in the generic `TheoryData<>` is known to not be
serializable.

## Reason for rule

Non-serializable data makes it impossible for the developer to run individual data rows inside of
Visual Studio's Test Explorer.

## How to fix violations

To fix a violation of this rule, use data that is known to be serializable. This includes all the
supported built-in types (listed below) or any type which implements `IXunitSerializable`, as well as
arrays of any supported type and nullable versions of any supported value type.

Supported built-in types (as of v2 `2.7.1` and v3 `0.1.1-pre.392`) include:

- `BigInteger`
- `bool`
- `byte` and `sbyte`
- `char`
- `DateTime`, `DateTimeOffset`, and `TimeSpan`
- `DateOnly` and `TimeOnly` (.NET 8+ only)
- Enum values (unless you're using .NET Framework and they live in the GAC)
- `decimal`
- `float` and `double`
- `int` and `uint`
- `long` and `ulong`
- `short` and `ushort`
- `string`
- `Type`

Additional built-in types supported for v3 (as of `0.5.0`) include:

- `Guid`
- `Index` and `Range` (.NET 8+ only)
- `Uri`
- `Version`

## Examples

### Violates

```csharp
using Xunit;

public sealed class TestData { }

public class TestClass
{
    public static TheoryData<TestData> DataSource = [];

    [Theory]
    [MemberData(nameof(DataSource))]
    public void TestMethod(TestData data) { }
}
```

### Does not violate

```csharp
using Xunit;

public class TestClass
{
    public static TheoryData<int, string> DataSource = [];

    [Theory]
    [MemberData(nameof(DataSource))]
    public void TestMethod(int intValue, string stringValue) { }
}
```

```csharp
using Xunit;
using Xunit.Abstractions;  // for IXunitSerializable in v2
using Xunit.Sdk;           // for IXunitSerializable in v3

public sealed class TestData : IXunitSerializable { }

public class TestClass
{
    public static TheoryData<TestData> DataSource = [];

    [Theory]
    [MemberData(nameof(DataSource))]
    public void TestMethod(TestData data) { }
}
```
