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

Developers using xUnit.net v3 `0.6.0` or later can also implement an external serializer by implementing
`IXunitSerializer` and registering it in their test assembly. For more information, see
[the v3 documentation on custom serialization](/docs/getting-started/v3/custom-serialization).

Supported built-in types for v2 (as of `2.7.1`) and v3 include:

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
