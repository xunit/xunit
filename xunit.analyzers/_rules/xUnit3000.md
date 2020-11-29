---
title: xUnit3000
description: Test case classes must derive directly or indirectly from Xunit.LongLivedMarshalByRefObject
category: Extensibility
severity: Error
---

## Cause

Test case classes must derive from LongLivedMarshalByRefObject to correctly support both desktop CLR (with
app domains) and non-desktop CLR (without app domains).

## Reason for rule

xUnit.net test case objects must live in the execution app domain, and must be able to live longer than the
default 5 minutes.

## How to fix violations

To fix a violation of this rule, use `Xunit.LongLivedMarshalByRefObject` as the base class for your test case class.

## Examples

### Violates

```csharp
using Xunit.Abstractions;

public class MyTestCase : ITestCase
{
	// ...implementation of test case...
}
```

### Does not violate

```csharp
using Xunit;
using Xunit.Abstractions;

public class MyTestCase : LongLivedMarshalByRefObject, ITestCase
{
	// ...implementation of test case...
}
```
