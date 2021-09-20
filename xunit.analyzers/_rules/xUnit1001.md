---
title: xUnit1001
description: Fact methods cannot have parameters
category: Usage
severity: Error
---

## Cause

A fact method has one or more parameters.

## Reason for rule

A fact method is a non-parameterized test. xUnit.net will raise a runtime error if it sees a fact method with a non-empty parameter list.

## How to fix violations

To fix a violation of this rule, remove the parameters from the fact method. Alternatively, change the `[Fact]` attribute to `[Theory]`.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1001
{
    [Fact]
    public void TestMethod(int _)
    { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1001
{
    [Fact]
    public void TestMethod()
    { }
}
```

```csharp
using Xunit;

public class xUnit1001
{
    [Theory]
    public void TestMethod(int _)
    { }
}
```
