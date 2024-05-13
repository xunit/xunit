---
title: xUnit1014
description: MemberData should use nameof operator for member name
category: Usage
severity: Warning
---

## Cause

This rule is triggered by `[MemberData]` which uses a string for the member name.

## Reason for rule

Using `nameof` instead of a string literal value allows rename refactoring to change the values in `[MemberData]` in the event the developer renames the data member name.

## How to fix violations

To fix a violation of this rule, convert from the string literal to `nameof`.

## Examples

### Violates

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1014
{
    public static IEnumerable<object[]> TestData;

    [Theory]
    [MemberData("TestData")]
    public void TestMethod(string greeting, int age) { }
}
```

Example(s) of code that violates the rule.

### Does not violate

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1014
{
    public static IEnumerable<object[]> TestData;

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestMethod(string greeting, int age) { }
}
```
