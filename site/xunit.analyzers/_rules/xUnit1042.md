---
title: xUnit1042
description: The member referenced by the MemberData attribute returns untyped data rows
category: Usage
severity: Info
v2: true
v3: true
---

## Cause

xUnit.net offers a strongly-typed `TheoryData<>` class which can be used instead of `IEnumerable<object[]>`
when returning member data. For v3 projects, you can also use `IEnumerable<TheoryDataRow<>>` and
`IAsyncEnumerable<TheoryDataRow<>>`, as well as supporting `Task<>` or `ValueTask<>` around the
entire data set.

## Reason for rule

Using `TheoryData<>` or `TheoryDataRow<>` provides a type-safe way to provide member data for
theories. The compiler can indicate when you have provided incompatible data, and several other analyzers
will help ensure your `TheoryData<>` or `TheoryDataRow<>` type usage matches your test method parameters.

## How to fix violations

To fix a violation of this rule in v2, replace `IEnumerable<object[]>` with `TheoryData<>`.

To fix a violation of this rule in v3, replace `IEnumerable<object[]>`, `IEnumerable<ITheoryDataRow>`, or `IEnumerable<TheoryDataRow>` with either `TheoryData<>` or `IEnumerable<TheoryDataRow<>>`. You may also use `IAsyncEnumerable<>`, and you may wrap the return value in either `Task<>` or `ValueTask<>`.

## Examples

### Violates

#### For v2 and v3

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

#### For v3 only

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1042
{
    public static IEnumerable<TheoryDataRow> TestData = [new(42), new(2112)];

    [Theory]
    [MemberData(nameof(TestData))]
    public static void TestMethod(int _)
    { }
}
```

### Does not violate

#### For v2 and v3

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

#### For v3

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1042
{
    public static IEnumerable<TheoryDataRow<int>> TestData = [new(42), new(2112)];

    [Theory]
    [MemberData(nameof(TestData))]
    public static void TestMethod(int _)
    { }
}
```
