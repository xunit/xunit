---
title: xUnit1019
description: MemberData must reference a member providing a valid data type
category: Usage
severity: Error
---

## Cause

This rule is triggered when your `[MemberData]` attribute points to a member does not have a valid return type (`IEnumerable<object[]>` or something compatible with it, like `TheoryData<>`).

## Reason for rule

`[MemberData]` attributes which do not point to valid data sources will fail at runtime.

## How to fix violations

To fix a violation of this rule, update the data member to have an appropriate return type.

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
