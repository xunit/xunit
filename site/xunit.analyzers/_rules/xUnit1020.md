---
title: xUnit1020
description: MemberData must reference a property with a getter
category: Usage
severity: Error
---

## Cause

This rule is triggered when your `[MemberData]` attribute points to a property without a public getter.

## Reason for rule

`[MemberData]` attributes which point to write-only properties will fail at runtime, since the property value cannot be read to retrieve the data.

## How to fix violations

To fix a violation of this rule, update the data member to have a getter.

## Examples

### Violates

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1020
{
    public static IEnumerable<object[]> TestData { private get; set; }

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestMethod(string greeting, int age) { }
}
```

### Does not violate

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1020
{
    public static IEnumerable<object[]> TestData { get; set; }

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestMethod(string greeting, int age) { }
}
```
