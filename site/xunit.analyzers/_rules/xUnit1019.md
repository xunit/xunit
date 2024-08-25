---
title: xUnit1019
description: MemberData must reference a member providing a valid data type
category: Usage
severity: Error
v2: true
v3: true
---

## Cause

This rule is triggered when your `[MemberData]` attribute points to a member does not have a valid return type (`IEnumerable<object[]>` or something compatible with it, like `TheoryData<>`). For v3 tests, this also includes support for `IAsyncEnumerable<>` as well as supporting `ITheoryDataRow` in addition to `object[]` to represent a single row of data.

## Reason for rule

`[MemberData]` attributes which do not point to valid data sources will fail at runtime.

## How to fix violations

To fix a violation of this rule, update the data member to have an appropriate return type.

<p class="note">Using the types <code>object[]</code> or <code>ITheoryRowData</code> will trigger <a href="xUnit1042">xUnit1042</a> because they are not strongly typed. It is strongly suggested that you choose to use <code>TheoryData&lt;&gt;</code> for v2/v3, or <code>IEnumerable&lt;TheoryDataRow&lt;&gt;&gt;</code>/<code>IAsyncEnumerable&lt;TheoryDataRow&lt;&gt;&gt;</code> for v3, as those provide compiler-level type safety and additional code analysis is possible to ensure the types of the test method parameters match the generic types of your theory data.</p>

## Examples

### Violates

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1019
{
    public static IEnumerable<object> TestData;

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestMethod(string greeting, int age) { }
}
```

### Does not violate

#### For v2 and v3

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1019
{
    public static IEnumerable<object[]> TestData;

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestMethod(string greeting, int age) { }
}
```

#### For v3 only

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1019
{
    public static IEnumerable<ITheoryDataRow> TestData;

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestMethod(string greeting, int age) { }
}
```

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1019
{
    public static IAsyncEnumerable<object[]> TestData;

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestMethod(string greeting, int age) { }
}
```

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1019
{
    public static IAsyncEnumerable<ITheoryDataRow> TestData;

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestMethod(string greeting, int age) { }
}
```
