---
title: xUnit1024
description: Test methods cannot have overloads
category: Usage
severity: Error
---

## Cause

This rule is triggered when you have more than one public methods with the same name, and at least one of them is marked as a test method.

## Reason for rule

xUnit.net does not support method overloads for test methods. Any test method must have a unique name in the test class.

## How to fix violations

To fix a violation of this rule, you may:

* Rename the extra method(s)
* Delete the extra method(s)
* Mark the extra method(s) with visibility other than `public`

## Examples

### Violates

```csharp
using Xunit;

public class xUnit1024
{
    [Fact]
    public void TestMethod() { }

    public void TestMethod(int age) { }
}
```

### Does not violate

```csharp
using Xunit;

public class xUnit1024
{
    [Fact]
    public void TestMethod() { }

    public void NonTestMethod(int age) { }
}
```

```csharp
using Xunit;

public class xUnit1024
{
    [Fact]
    public void TestMethod() { }
}
```

```csharp
using Xunit;

public class xUnit1024
{
    [Fact]
    public void TestMethod() { }

    private void TestMethod(int age) { }
}
```
