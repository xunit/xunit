---
layout: default
title: "Custom theory data serialization for xUnit.net v3"
breadcrumb: Documentation
---

# Custom theory data serialization for xUnit.net v3

_Last updated: 2024 December 16_

As of version `0.5.0-pre.27`, we are now supporting a second way to implement custom theory data serialization for xUnit.net v3. Before discussing the new feature in v3 Core Framework, we will review why custom theory data serialization exists, and how it is implemented today in the v2 Core Framework.

## Why custom theory data serialization?

Our VSTest adapter (implemented in `xunit.runner.visualstudio`) provides support for running xUnit.net tests for Test Explorer, in addition to supporting `dotnet test`. It is the Test Explorer support where theory data serialization comes into play.

The design of VSTest, the underlying support system used by Test Explorer, separates discovery of tests from execution of tests (across an arbitrarily long time period, and even across discovery processes vs. execution processes). When a test project is built, the system discovers tests and hands them to VSTest, which in turn provides them to Test Explorer (as well as decorating your source code to show current test status via CodeLens). You can choose to run one, several, or all of the tests via several gestures in the system.

In order to show each theory data row as a runnable entity in Test Explorer, we return test information to VSTest which completely describes what we need to run that test, including the data in the data row. In order to do this, we must serialize the data when handing the test case to VSTest, so that it can later hand us back that test case to run the requested test. We deserialize the theory data row so that we can provide it to the test method to run the test.

## What can be serialized by default?

We have built-in support for serializing intrinsic data types, as well as several commonly used system types.

Intrinsics (C# names):

* `bool`
* `byte`
* `char`
* `decimal`
* `double`
* `float`
* `int`
* `long`
* `sbyte`
* `short`
* `string`
* `uint`
* `ulong`

System types:

* `System.DateOnly`
* `System.DateTime`
* `System.DateTimeOffset`
* `System.Guid`
* `System.Index`
* `System.Numerics.BigInteger`
* `System.Range`
* `System.TimeOnly`
* `System.TimeSpan`
* `System.Type`
* `System.Uri`
* `System.Version`

Additional supported values:

* Arrays of serializable values
* Enum values
* Values which implement both [`IFormattable`](https://learn.microsoft.com/dotnet/api/system.iformattable) and [`IParsable<TSelf>`](https://learn.microsoft.com/dotnet/api/system.iparsable-1) _(starting with v3 1.1.0 or later)_.
* `null` values

In order to support developers wanting to be able to run individual theory data rows, we also added the ability to provide your own custom serialization for your own custom data types.

## Serialization support in v2

For v2 Core Framework, we introduced `IXunitSerializable`, an interface which you could implement on any custom data type, that we would use to serialize your theory data. To implement this, you were required to:

* Provide a parameterless public constructor so we could create an "empty" object to deserialize into
* Implement `IXunitSerializable.Serialize` to store data values
* Implement `IXunitSerializable.Deserialize` to retrieve stored data values

The underlying implementation of the data store itself used the serialization system, so not only could `IXunitSerializable` objects serialize data values from our built-in list of intrinsics and system types, but also any other type which itself implements `IXunitSerializable`.

This system is effective, but limited: you need to be able to create a custom data object that implements `IXunitSerializable` to participate in serialization (as well as willing to include test framework serialization support for types which may not be exclusive to your test project, such as data transfer objects in production code).

## Serialization support in v3

For v3 Core Framework, we continue to support `IXunitSerializable`.

We have also added a new interface, `IXunitSerializer`, that can be separately implemented to provide serialization support for any type, regardless of whether you control it or not. The implementation of `IXunitSerializer` is relatively straight forward:

* Implement `bool IsSerializable(Type, object?, out string?)` to determine if the value is serializable (including a reason message if it's not)
* Implement `string Serialize(object)` to serialize a value
* Implement `object Deserialize(Type, string)` to deserialize a previously serialized value
* Provide a public parameterless constructor so the serializer can be created (and cached)

Note that the call to `Serialize` will never pass a `null` object (since the built-in serialization system already knows how to serialize `null` values), and as such, the call to `Deserialize` is expected to return a non-`null` value. The original concrete type of the object that was serialized is stored, so that we can provide the type to `Deserialize` so that it knows what concrete type it is attempting to reconstruct.

One implementation of `IXunitSerializer` can serialize more than one data type, if it so chooses. Registration is done via an assembly-level attribute which registers the serializer type, and one or more types that it can serialize:

```csharp
[assembly: RegisterXunitSerializer(typeof(MySerializer), typeof(SupportedType1), typeof(SupportedType2), ...]
```

These supported data types can be concrete types or interfaces, and polymorphism is supported (so if you can serialize an entire type hierarchy, you need only register the base type as supported). For supported data types, closed generics are supported, but open generics are not. When a concrete supported data type is provided, when attempting to locate the correct serializer, an exact type match will always take preference over a polymorphic match, and the first serializer that it claims it can polymorphically support the given type "wins" (i.e., there is no attempt to reconcile when multiple serializers might support a given type polymorphically). Since the serializer is responsible for determining how to create the object for deserialization, xUnit.net makes no requirements on the supported data types with respect to constructors.

_**Note:** You cannot override the serialization for any of the built-in types listed above; such a registration will fail with a warning, and the serializer will not be used. You will receive a similar warning if two serializers attempt to register for the same supported data type, with the first processed registration "winning" and subsequent registrations being ignored._
