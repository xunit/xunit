---
title: xUnit3001
description: Classes that implement IXunitSerializable or are marked with [JsonTypeID] must have a public parameterless constructor
category: Extensibility
severity: Error
---

## Cause

Classes which implement `Xunit.Abstractions.IXunitSerializable` (v2) or `Xunit.Sdk.IXunitSerializable` (v3), or are tagged with `[JsonTypeID]` (v3) are required to have a public parameterless constructor.

## Reason for rule

When xUnit.net deserializes objects, it must construct them using the public parameterless constructor. The body of the constructor should be empty, since all values will come to the object via `IXunitSerializable.Deserialize` (or `IJsonDeserializable.FromJson`).

## How to fix violations

Add a public parameterless empty-bodied constructor, and mark it with `[System.Obsolete]` so users don't call it directly. You may keep any other public constructors with parameters.

## Examples

### Violates

#### v2 Core Framework

```csharp
using Xunit.Abstractions;

public class xUnit3001 : IXunitSerializable
{
    public xUnit3001(int _) { }

    public void Deserialize(IXunitSerializationInfo info) { }

    public void Serialize(IXunitSerializationInfo info) { }
}
```

#### v3 Core Framework

```csharp
using Xunit.Sdk;

public class xUnit3001 : IXunitSerializable
{
    public xUnit3001(int _) { }

    public void Deserialize(IXunitSerializationInfo info) { }

    public void Serialize(IXunitSerializationInfo info) { }
}
```

```csharp
[JsonTypeID("json-type")]
public class xUnit3001 : IJsonSerializable, IJsonDeserializable
{
    public xUnit3001(int _) { }

    public void FromJson(IReadOnlyDictionary<string, object?> root) { }

    public string ToJson() => "{}";
}
```

### Does not violate

#### v2 Core Framework

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

#### v3 Core Framework

```csharp
using System;
using Xunit.Sdk;

public class xUnit3001 : IXunitSerializable
{
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public xUnit3001() { }

    public xUnit3001(int _) { }

    public void Deserialize(IXunitSerializationInfo info) { }

    public void Serialize(IXunitSerializationInfo info) { }
}
```

```csharp
[JsonTypeID("json-type")]
public class xUnit3001 : IJsonSerializable, IJsonDeserializable
{
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public xUnit3001() { }

    public xUnit3001(int _) { }

    public void FromJson(IReadOnlyDictionary<string, object?> root) { }

    public string ToJson() => "{}";
}
```
