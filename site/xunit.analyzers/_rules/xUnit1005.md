---
title: xUnit1005
description: Fact methods should not have test data
category: Usage
severity: Warning
---

## Cause

A fact method has one or more attributes that provide test data.

## Reason for rule

Unlike theory methods, fact methods do not have any parameters. Providing a fact method with test data is therefore pointless, as there is no way to actually pass that data to the test method.

## How to fix violations

To fix a violation of this rule, either:

* Turn the fact method into a theory method by replacing `[Fact]` with `[Theory]`, if your test method is parameterized.

* Remove any attributes that provide test data, if your test method is not parameterized. (That is, remove all attributes of a type deriving from `DataAttribute`, such as `[ClassData]`, `[InlineData]`, or `[MemberData]`.)

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1005
{
    [Fact, InlineData(1)]
    public void TestMethod()
    { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1005
{
    [Fact]
    public void TestMethod()
    { }
}
```

```csharp
using Xunit;

public class xUnit1005
{
    [Theory, InlineData(1)]
    public void TestMethod(int _)
    { }
}
```
