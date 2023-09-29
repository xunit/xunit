---
title: xUnit3000
description: Classes which cross AppDomain boundaries must derive directly or indirectly from LongLivedMarshalByRefObject
category: Extensibility
severity: Error
---

## Cause

Classes which may cross AppDomain boundaries must derive from LongLivedMarshalByRefObject to correctly support both .NET Framework (with
app domains) and .NET Core (without app domains).

## Reason for rule

Classes that implement one of the cross-AppDomain interface types (from the `Xunit.Abstractions` namespace) must be able to be proxied across
AppDomain boundaries, and must be able to survive for longer than the default 5 minutes.

## How to fix violations

To fix a violation of this rule, use `LongLivedMarshalByRefObject` as the base class for your class. If your class lives on the runner side, then
use `Xunit.Sdk.LongLivedMarshalByRefObject`; if your class lives on the execution side, use `Xunit.LongLivedMarshalByRefObject`.

## Examples

### Violates

```csharp
using Xunit.Abstractions;

public class xUnit3000 : ITestCase
{
    // ...implementation of test case...
}
```

### Does not violate

```csharp
using Xunit;
using Xunit.Abstractions;

public class xUnit3000 : LongLivedMarshalByRefObject, ITestCase
{
    // ...implementation of test case...
}
```
