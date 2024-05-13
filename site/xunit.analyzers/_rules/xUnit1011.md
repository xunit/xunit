---
title: xUnit1011
description: There is no matching method parameter
category: Usage
severity: Error
---

## Cause

This rule is triggered when you have more test data in your `[InlineData]` attribute then parameters on your test method.

## Reason for rule

Having excess test data will cause the theory to fail at runtime.

## How to fix violations

To fix a violation of this rule, you may:

* Remove the unused data
* Add a parameter to the test method

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1011
{
    [Theory]
    [InlineData("Hello world", 42)]
    public void TestMethod(string greeting) { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1011
{
    [Theory]
    [InlineData("Hello world")]
    public void TestMethod(string greeting) { }
}
```

```csharp
using Xunit;

public class xUnit1011
{
    [Theory]
    [InlineData("Hello world", 42)]
    public void TestMethod(string greeting, int age) { }
}
```
