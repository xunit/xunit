using System;

namespace Xunit.Sdk;

/// <summary>
/// Used to decorate xUnit.net test assemblies to register an external serializer for
/// one or more supports types to serialize.
/// </summary>
/// <remarks>Serializer registration attributes are only valid at the assembly level.</remarks>
public interface IRegisterXunitSerializerAttribute
{
	/// <summary>
	/// Gets the type of the serializer.
	/// </summary>
	/// <remarks>
	/// The serializer type must implement <see cref="IXunitSerializer"/>.
	/// </remarks>
	Type SerializerType { get; }

	/// <summary>
	/// Gets the types that are supported by the serializer.
	/// </summary>
	/// <remarks>
	/// When searching for a serializer to deserialize a value, exact type matches are
	/// given higher priority than compatible type matches, and if more than one serializer
	/// can support a given type based on compatible type match, then one will be chosen
	/// arbitrarily to support the deserialization.
	/// </remarks>
	Type[] SupportedTypesForSerialization { get; }
}
