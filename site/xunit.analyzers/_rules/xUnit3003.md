---
title: xUnit3003
description: Classes which extend FactAttribute (directly or indirectly) should provide a public constructor for source information
category: Extensibility
severity: Warning
v2: false
v3: true
---

## Cause

Source location information in xUnit.net v3 (starting with build `3.0.0-pre.15`) is collected by way of constructor arguments that are supplied automatically by the compiler. Classes which derive directly or indirectly from `FactAttribute` should have a constructor which accepts the source information and passes it along to the base class.

## Reason for rule

Failure to provide the source information via the compiler will result in tests not having source information in Microsoft Testing Platform mode, including Test Explorer and `dotnet test`.

## How to fix violations

To fix a violation of this rule, add a constructor which accepts and passes along the source information.

## Examples

### Violates

```csharp
using Xunit;

public class CustomFactAttribute : FactAttribute
{
    // ...
}
```

```csharp
using Xunit;

public class CustomTheoryAttribute : TheoryAttribute
{
    // ...
}
```

### Does not violate

```csharp
using System.Runtime.CompilerServices;
using Xunit;

public class CustomFactAttribute(
    [CallerFilePath] string? sourceFilePath = null,
    [CallerLineNumber] int sourceLineNumber = -1)
      : FactAttribute(sourceFilePath, sourceLineNumber)
{
    // ...
}
```

```csharp
using System.Runtime.CompilerServices;
using Xunit;

public class CustomTheoryAttribute(
    [CallerFilePath] string? sourceFilePath = null,
    [CallerLineNumber] int sourceLineNumber = -1)
      : TheoryAttribute(sourceFilePath, sourceLineNumber)
{
    // ...
}
```
