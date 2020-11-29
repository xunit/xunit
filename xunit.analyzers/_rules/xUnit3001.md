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

Add a public parameterless empty-bodied constructor, and mark it with `[System.Obsolete]` so users don't call it directly.

## Examples

### Violates

```csharp
using Xunit.Abstractions;

public class MySerializableObject : IXunitSerializable
{
	public void Deserialize(IXunitSerializationInfo info)
	{
		// ...implementation...
	}

	public void Serialize(IXunitSerializationInfo info)
	{
		// ...implementation...
	}
}
```

### Does not violate

```csharp
using System;
using Xunit.Abstractions;

public class MySerializableObject : IXunitSerializable
{
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public MySerializableObject() { }

	public void Deserialize(IXunitSerializationInfo info)
	{
		// ...implementation...
	}

	public void Serialize(IXunitSerializationInfo info)
	{
		// ...implementation...
	}
}
```
