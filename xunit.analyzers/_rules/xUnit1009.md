---
title: xUnit1009
description: InlineData values must match the number of method parameters
category: Usage
severity: Error
---

## Cause

This rule is triggered when you don't have enough test data in your `[InlineData]` attribute to match the number of parameters on your test method.

## Reason for rule

A theory which has insufficient data to cover all the tests will fail when you attempt to run it because of the missing data.

## How to fix violations

To fix a violation of this rule, you may:

* Add data to the `[InlineData]` attribute
* Remove unused parameters from the test method
* Add a default parameter value on the test method parameter

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1009
{
    [Theory]
    [InlineData("Hello world")]
    public void TestMethod(string greeting, int age) { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1009
{
    [Theory]
    [InlineData("Hello world", 42)]
    public void TestMethod(string greeting, int age) { }
}
```

```csharp
using Xunit;

public class xUnit1009
{
    [Theory]
    [InlineData("Hello world")]
    public void TestMethod(string greeting) { }
}
```

```csharp
using Xunit;

public class xUnit1009
{
    [Theory]
    [InlineData("Hello world")]
    public void TestMethod(string greeting, int age = 42) { }
}
```
