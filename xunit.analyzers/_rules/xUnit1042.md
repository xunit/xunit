---
title: xUnit1042
description: The member referenced by the MemberData attribute returns untyped data rows
category: Usage
severity: Info
---

## Cause

xUnit.net offers a strongly-typed `TheoryData<>` class which can be used instead of `IEnumerable<object[]>`
when returning member data.

## Reason for rule

Using `TheoryData<>` instead of `IEnumerable<object[]>` provides a type-safe way to provide member data for
theories. The compiler can indicate when you have provided incompatible data, and several other analyzers
will help ensure your `TheoryData<>` type usage matches your test method parameters.

## How to fix violations

To fix a violation of this rule, replace `IEnumerable<object[]>` with `TheoryData<>`.

## Examples

### Violates

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1042
{
    public static IEnumerable<object[]> TestData = new[] { new object[] { 42 }, new object[] { 2112 } };

    [Theory]
    [MemberData(nameof(TestData))]
    public static void TestMethod(int _)
    { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1042
{
    public static TheoryData<int> TestData = new() { 42, 2112 };

    [Theory]
    [MemberData(nameof(TestData))]
    public static void TestMethod(int _)
    { }
}
```
