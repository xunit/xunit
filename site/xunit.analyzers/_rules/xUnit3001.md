---
title: xUnit3001
description: Classes that implement Xunit.Abstractions.IXunitSerializable must have a public parameterless constructor
category: Extensibility
severity: Error
---

## Cause

Classes which implement `Xunit.Abstractions.IXunitSerializable` are required to have a public parameterless constructor.

## Reason for rule

When xUnit.net deserializes objects, it must construct them using the public parameterless constructor. The body of the constructor
should be empty, since all values will come to the object via `IXunitSerializable.Deserialize`.

## How to fix violations

Add a public parameterless empty-bodied constructor, and mark it with `[System.Obsolete]` so users don't call it directly. You may
keep any other public constructors with parameters.

## Examples

### Violates

```csharp
using Xunit.Abstractions;

public class xUnit3001 : IXunitSerializable
{
    public xUnit3001(int _) { }

    public void Deserialize(IXunitSerializationInfo info) { }

    public void Serialize(IXunitSerializationInfo info) { }
}
```

### Does not violate

```csharp
using System;
using Xunit.Abstractions;

public class xUnit3001 : IXunitSerializable
{
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public xUnit3001() { }

    public xUnit3001(int _) { }

    public void Deserialize(IXunitSerializationInfo info) { }

    public void Serialize(IXunitSerializationInfo info) { }
}
```
