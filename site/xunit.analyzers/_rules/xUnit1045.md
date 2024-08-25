---
title: xUnit1045
description: Avoid using TheoryData type arguments that might not be serializable
category: Usage
severity: Info
v2: true
v3: true
---

## Cause

A violation of this rule occurs when a type in the generic `TheoryData<>` might not be
serializable.

## Reason for rule

Non-serializable data makes it impossible for the developer to run individual data rows inside of
Visual Studio's Test Explorer.

## How to fix violations

To fix a violation of this rule, use data that is known to be serializable. This includes all the
supported built-in types (listed below) or any type which implements `IXunitSerializable`, as well as
arrays of any supported type and nullable versions of any supported value type. A type might or
might not be serializable if it's an interface (that does not derive from `IXunitSerializable`)
or an unsealed class or struct (that does not implement `IXunitSerializable`).

Supported built-in types (as of v2 `2.7.1` and v3 `0.1.1-pre.392`) include:

- `BigInteger`
- `bool`
- `byte` and `sbyte`
- `char`
- `DateTime`, `DateTimeOffset`, and `TimeSpan`
- `DateOnly` and `TimeOnly` (.NET 6+ only)
- Enum values (unless you're using .NET Framework and they live in the GAC)
- `decimal`
- `float` and `double`
- `int` and `uint`
- `long` and `ulong`
- `short` and `ushort`
- `string`
- `Type`

## Examples

### Violates

```csharp
using System;
using Xunit;

public class TestClass
{
    public static TheoryData<IDisposable> DataSource = [];

    [Theory]
    [MemberData(nameof(DataSource))]
    public void TestMethod(IDisposable disposable) { }
}
```

### Does not violate

```csharp
using System;
using Xunit;
using Xunit.Abstractions;  // for IXunitSerializable in v2
using Xunit.Sdk;           // for IXunitSerializable in v3

public interface IDisposableSerializable : IDisposable, IXunitSerializable { }

public class TestClass
{
    public static TheoryData<IDisposableSerializable> DataSource = [];

    [Theory]
    [MemberData(nameof(DataSource))]
    public void TestMethod(IDisposableSerializable disposable) { }
}
```
