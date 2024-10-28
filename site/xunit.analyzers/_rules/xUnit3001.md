---
title: xUnit3001
description: Classes that are marked as serializable (or created by the test framework at runtime) must have a public parameterless constructor
category: Extensibility
severity: Error
v2: true
v3: true
---

## Cause

Classes which are created by the test framework at runtime, including classes which are serialized, must have a public parameterless constructor.

This includes class which implement:

* `Xunit.Abstractions.IXunitSerializable` (v2)
* `Xunit.Sdk.IXunitSerializable` (v3)
* `Xunit.Sdk.IXunitSerializer` (v3)
* `Xunit.Runner.Common.IRunnerReporter` (v3)

It also includes classes tagged with `[Xunit.Sdk.JsonTypeID]` (v3).

## Reason for rule

Some classes are created using reflection by the test framework at runtime, and require a parameterless constructor.

When those types are serialized, the body of the constructor should be empty, since all values will come to the object via the deserialization method (`IXunitSerializable.Deserialize` or `IJsonDeserializable.FromJson`).

## How to fix violations

Add a public parameterless constructor. If the type is serialized, make the constructor empty-bodied, and mark it with `[System.Obsolete]` so users don't call it directly.

You may keep any other public constructors with parameters.

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
[JsonTypeID("json-type")]
public class xUnit3001 : IJsonSerializable, IJsonDeserializable
{
    public xUnit3001(int _) { }

    public void FromJson(IReadOnlyDictionary<string, object?> root) { }

    public string? ToJson() => "{}";
}
```

```csharp
using System;
using System.Threading.Tasks;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class xUnit3001 : IRunnerReporter
{
    public xUnit3001(int _) { }

    public bool CanBeEnvironmentallyEnabled => false;

    public string Description => string.Empty;

    public bool ForceNoLogo => false;

    public bool IsEnvironmentallyEnabled => false;

    public string? RunnerSwitch => "custom";

    public ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
        IRunnerLogger logger,
        IMessageSink? diagnosticMessageSink) =>
            throw new NotImplementedException();
}
```

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
using System;
using Xunit.Sdk;

public class xUnit3001 : IXunitSerializer
{
    public xUnit3001(int _) { }

    public object Deserialize(Type type, string serializedValue) => new();

    public bool IsSerializable(Type type, object? value) => true;

    public string Serialize(object value) => string.Empty;
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
[JsonTypeID("json-type")]
public class xUnit3001 : IJsonSerializable, IJsonDeserializable
{
    [Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
    public xUnit3001() { }

    public xUnit3001(int _) { }

    public void FromJson(IReadOnlyDictionary<string, object?> root) { }

    public string? ToJson() => "{}";
}
```

```csharp
using System;
using System.Threading.Tasks;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class xUnit3001 : IRunnerReporter
{
    public xUnit3001() { }

    public xUnit3001(int _) { }

    public bool CanBeEnvironmentallyEnabled => false;

    public string Description => string.Empty;

    public bool ForceNoLogo => false;

    public bool IsEnvironmentallyEnabled => false;

    public string? RunnerSwitch => "custom";

    public ValueTask<IRunnerReporterMessageHandler> CreateMessageHandler(
        IRunnerLogger logger,
        IMessageSink? diagnosticMessageSink) =>
            throw new NotImplementedException();
}
```

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
using System;
using Xunit.Sdk;

public class xUnit3001 : IXunitSerializer
{
    public xUnit3001() { }

    public xUnit3001(int _) { }

    public object Deserialize(Type type, string serializedValue) => new();

    public bool IsSerializable(Type type, object? value) => true;

    public string Serialize(object value) => string.Empty;
}
```
