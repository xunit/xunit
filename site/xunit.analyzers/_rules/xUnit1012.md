---
title: xUnit1012
description: Null should not be used for value type parameters
category: Usage
severity: Warning
---

## Cause

This rule is trigged by having a `null` value in your `[InlineData]` for a value type parameter.

## Reason for rule

Value types are incompatible with `null` values.

## How to fix violations

To fix a violation of this rule, you may:

* Replace the `null` value with a non-`null` value
* Convert the parameter type to a nullable value type
* Convert the parameter type to a reference type

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1012
{
    [Theory]
    [InlineData(null)]
    public void TestMethod(int _) { }
}
```

If nullable reference types are enabled, this also violates:

```csharp
using Xunit;

public class xUnit1012
{
    [Theory]
    [InlineData(null)]
    public void TestMethod(object _) { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1012
{
    [Theory]
    [InlineData(42)]
    public void TestMethod(int _) { }
}
```

```csharp
using Xunit;

public class xUnit1012
{
    [Theory]
    [InlineData(null)]
    public void TestMethod(int? _) { }
}
```

If nullable reference types are enabled, parameters must be decorated to receive `null` values:

```csharp
using Xunit;

public class xUnit1012
{
    [Theory]
    [InlineData(null)]
    public void TestMethod(object? _) { }
}
```
