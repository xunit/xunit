---
title: xUnit1043
description: Constructors on classes derived from FactAttribute must be public when used on test methods
category: Usage
severity: Error
---

## Cause

A violation of this rule occurs when a test method tries to use a `FactAttribute`-derived attribute through
a non-`public` constructor.

## Reason for rule

Although the compiler will allow a user to call an `internal` (or `protected internal`) constructor, the xUnit.net
framework requires that the attribute constructor used is public.

## How to fix violations

To fix a violation of this rule, either use an existing public constructor, or change the desired constructor
to `public` visibility.

## Examples

### Violates

```csharp
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomFactAttribute : FactAttribute
{
    internal CustomFactAttribute() { }
}

public class xUnit1043
{
    [CustomFact]
    public void TestMethod() { }
}
```

### Does not violate

```csharp
using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomFactAttribute : FactAttribute
{
    public CustomFactAttribute() { }
}

public class xUnit1043
{
    [CustomFact]
    public void TestMethod() { }
}
```
