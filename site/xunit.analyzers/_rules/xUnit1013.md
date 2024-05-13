---
title: xUnit1013
description: Public method should be marked as test
category: Usage
severity: Warning
---

## Cause

This rule is trigger by having a public method in a test class that is not marked as a test.

## Reason for rule

It is frequently oversight to have a public method in a test class which isn't a test method.

## How to fix violations

To fix a violation of this rule, you may:

* Annotate the method with `Fact` or `Theory` attributes
* Change the visibility of the method to something other than `public`

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1013
{
    [Fact]
    public void TestMethod1() { }

    public void TestMethod2() { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1013
{
    [Fact]
    public void TestMethod1() { }

    [Fact]
    public void TestMethod2() { }
}
```

```csharp
using Xunit;

public class xUnit1013
{
    [Fact]
    public void TestMethod1() { }

    internal void TestMethod2() { }
}
```

## Opt-out for extension authors

Some xUnit.net extensions provide alternative attributes for annotating tests. Such attributes should be annotated with a marker attribute to prevent this rule from firing for valid usages of the extension. The marker attribute must be named `IgnoreXunitAnalyzersRule1013`, in any (or no) namespace.

```csharp
using System;
using Xunit;

public sealed class IgnoreXunitAnalyzersRule1013Attribute : Attribute { }

[IgnoreXunitAnalyzersRule1013]
public class CustomTestTypeAttribute : Attribute { }

public class xUnit1013
{
    [Fact]
    public void TestMethod() { }

    [CustomTestType]
    public void CustomTestMethod() { }
}
```
