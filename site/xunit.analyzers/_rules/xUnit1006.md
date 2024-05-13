---
title: xUnit1006
description: Theory methods should have parameters
category: Usage
severity: Warning
---

## Cause

A theory method does not have any parameters.

## Reason for rule

Theories are tests which are only true for a particular set of data. As such, they need to be provided with that data. xUnit.net does this by passing test data to a theory method via its parameters. Therefore, if the test method does not declare any parameters, there is no way for xUnit.net to pass the test data to the theory.

## How to fix violations

To fix a violation of this rule, either convert the test to a Fact, or add parameters to the test method. When adding parameters, the parameter count and types should match the provided test data.

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1006
{
    [Theory]
    [InlineData(12, "book")]
    public void TestMethod()
    { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1006
{
    [Fact]
    public void TestMethod()
    { }
}
```

```csharp
using Xunit;

public class xUnit1006
{
    [Theory]
    [InlineData(12, "book")]
    public void TestMethod(int quantity, string productType)
    { }
}
```
