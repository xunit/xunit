---
title: xUnit1046
description: Avoid using TheoryDataRow arguments that are not serializable
category: Usage
severity: Info
v2: false
v3: true
---

## Cause

A violation of this rule occurs when a value passed to the `TheoryDataRow` constructor is known to
not be serializable.

## Reason for rule

Non-serializable data makes it impossible for the developer to run individual data rows inside of
Visual Studio's Test Explorer.

## How to fix violations

To fix a violation of this rule, use data that is known to be serializable. This includes all the
supported built-in types (listed below) or any type which implements `IXunitSerializable`, as well as
arrays of any supported type and nullable versions of any supported value type.

Supported built-in types (as of v3 `0.1.1-pre.392`) include:

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

Additional built-in types supported for v3 (as of `0.5.0`) include:

- `Guid`
- `Index` and `Range` (.NET 6+ only)
- `Uri`
- `Version`

## Examples

### Violates

```csharp
using Xunit;

public sealed class TestData { }

public class MyClass
{
    public TheoryDataRow GetDataRow()
    {
        return new TheoryDataRow(new TestData());
    }
}
```

### Does not violate

```csharp
using Xunit;

public sealed class TestData { }

public class MyClass
{
    public TheoryDataRow GetDataRow()
    {
        return new TheoryDataRow(42, "Hello World!");
    }
}
```

```csharp
using Xunit;
using Xunit.Sdk;

public sealed class TestData : IXunitSerializable { }

public class MyClass
{
    public TheoryDataRow GetDataRow()
    {
        return new TheoryDataRow(new TestData());
    }
}
```
