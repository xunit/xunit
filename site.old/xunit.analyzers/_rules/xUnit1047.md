---
title: xUnit1047
description: Avoid using TheoryDataRow arguments that might not be serializable
category: Usage
severity: Info
v2: false
v3: true
---

## Cause

A violation of this rule occurs when a value passed to the `TheoryDataRow` constructor might not
be serializable.

## Reason for rule

Non-serializable data makes it impossible for the developer to run individual data rows inside of
Visual Studio's Test Explorer.

## How to fix violations

To fix a violation of this rule, use data that is known to be serializable. This includes all the
supported built-in types (listed below) or any type which implements `IXunitSerializable`, as well as
arrays of any supported type and nullable versions of any supported value type. A type might or
might not be serializable if it's an interface (that does not derive from `IXunitSerializable`)
or an unsealed class or struct (that does not implement `IXunitSerializable`).

Developers using build `0.6.0` or later can also implement an external serializer by implementing
`IXunitSerializer` and registering it in their test assembly. For more information, see
[the v3 documentation on custom serialization](/docs/getting-started/v3/custom-serialization).

Supported built-in types (as of v3 `0.5.0`) include:

- `BigInteger`
- `bool`
- `byte` and `sbyte`
- `char`
- `DateTime`, `DateTimeOffset`, and `TimeSpan`
- `DateOnly` and `TimeOnly` (.NET 6+ only)
- Enum values (unless you're using .NET Framework and they live in the GAC)
- `decimal`
- `float` and `double`
- `Guid`
- `Index` and `Range` (.NET 6+ only)
- `int` and `uint`
- `long` and `ulong`
- `short` and `ushort`
- `string`
- `Type`
- `Uri`
- `Version`

Additional built-in types supported for v3 (as of `0.5.0`) include:

- `Guid`
- `Index` and `Range` (.NET 6+ only)
- `Uri`
- `Version`

## Examples

### Violates

```csharp
using System;
using Xunit;

public class MyClass
{
    public TheoryDataRow GetDataRow()
    {
        IDisposable disposableValue = GetDisposableValue();

        return new TheoryDataRow(disposableValue);
    }
}
```

### Does not violate

```csharp
using System;
using Xunit;
using Xunit.Sdk;

public interface IDisposableSerializable : IDisposable, IXunitSerializable { }

public class MyClass
{
    public TheoryDataRow GetDataRow()
    {
        IDisposableSerializable disposableValue = GetDisposableValue();

        return new TheoryDataRow(disposableValue);
    }
}
```
