---
title: xUnit1008
description: Test data attribute should only be used on a Theory
category: Usage
severity: Warning
---

## Cause

This is triggered by forgetting to put the `[Theory]` attribute on a test method with data attributes.

## Reason for rule

If the test method does not have a `[Theory]` attribute on it, then the test will not run.

## How to fix violations

To fix a violation of this rule, either:

* Add a `[Theory]` attribute to the test method
* Remove the theory data attributes

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1008
{
    [InlineData(42)]
    public void TestMethod(int _) { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1008
{
    [Theory]
    [InlineData(42)]
    public void TestMethod(int _) { }
}
```

```csharp
using Xunit;

public class xUnit1008
{
    public void TestMethod(int _) { }
}
```

## How to suppress violations

```csharp
#pragma warning disable xUnit1008 // Test data attribute should only be used on a Theory
#pragma warning restore xUnit1008 // Test data attribute should only be used on a Theory
```
