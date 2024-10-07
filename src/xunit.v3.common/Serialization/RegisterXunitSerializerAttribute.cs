using System;

namespace Xunit.Sdk;

/// <summary>
/// Used to decorate xUnit.net test assemblies to register an external serializer for
/// one or more supports types to serialize.
/// </summary>
/// <param name="serializerType">The type of the serializer. Must implement <see cref="IXunitSerializer"/>.</param>
/// <param name="supportedTypesForSerialization">The types that are supported by the serializer.</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public sealed class RegisterXunitSerializerAttribute(
	Type serializerType,
	params Type[] supportedTypesForSerialization) :
		Attribute, IRegisterXunitSerializerAttribute
{
	/// <inheritdoc/>
	public Type SerializerType { get; } = serializerType;

	/// <inheritdoc/>
	public Type[] SupportedTypesForSerialization { get; } = supportedTypesForSerialization;
}
