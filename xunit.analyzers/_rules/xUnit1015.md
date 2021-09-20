---
title: xUnit1015
description: MemberData must reference an existing member
category: Usage
severity: Error
---

## Cause

This rule is triggered when a `[MemberData]` attribute does not point to a member.

## Reason for rule

The `[MemberData]` attribute must point to a valid data member to provide data, or else it will fail at runtime. Using `nameof` is a good way to get compiler and Intellisense help to ensure you are using a valid member name.

## How to fix violations

To fix a violation of this rule, update the name used in `[MemberData]` to point to at a data member.

## Examples

### Violates

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1015
{
    public static IEnumerable<object[]> TestData;

    [Theory]
    [MemberData("MyTestData")]
    public void TestMethod(string greeting, int age) { }
}
```

### Does not violate

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1015
{
    public static IEnumerable<object[]> TestData;

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestMethod(string greeting, int age) { }
}
```
