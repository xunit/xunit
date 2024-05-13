---
title: xUnit1021
description: MemberData should not have parameters if the referenced member is not a method
category: Usage
severity: Warning
---

## Cause

This rule is triggered when your `[MemberData]` attribute points to a non-method, but provides method arguments.

## Reason for rule

`[MemberData]` which points to a method can pass parameter values to that method; when it points to a field or a property, method parameters will be ignored (and thus should be removed).

## How to fix violations

To fix a violation of this rule, you may:

* Remove the method parameters from the `[MemberData]` attribute
* Convert the data member to a method with appropriate parameters

## Examples

### Violates

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1021
{
    public static IEnumerable<object[]> TestData { get; set; }

    [Theory]
    [MemberData(nameof(TestData), "Hello world", 123)]
    public void TestMethod(int _) { }
}
```

### Does not violate

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1021
{
    public static IEnumerable<object[]> TestData { get; set; }

    [Theory]
    [MemberData(nameof(TestData))]
    public void TestMethod(int _) { }
}
```

```csharp
using System.Collections.Generic;
using Xunit;

public class xUnit1021
{
    public static IEnumerable<object[]> TestData(string greeting, int age) { }

    [Theory]
    [MemberData(nameof(TestData), "Hello world", 123)]
    public void TestMethod(int _) { }
}
```
