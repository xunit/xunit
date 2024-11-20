using System;
using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Used to declare a specific test collection for a test class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class CollectionAttribute : Attribute, ICollectionAttribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionAttribute" /> class, with the
	/// given collection name.
	/// </summary>
	/// <param name="name">The test collection name.</param>
	public CollectionAttribute(string name) =>
		Name = name;

#pragma warning disable CA1019  // The type value is converted to exposed via the Name property

	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionAttribute" /> class based on
	/// a collection definition type, with an auto-generated name based on that type. Equivalent
	/// to using <see cref="CollectionAttribute{TCollectionDefinition}"/>.
	/// </summary>
	/// <param name="type">The type representing the collection fixture.</param>
	public CollectionAttribute(Type type)
	{
		Name = GetCollectionNameForType(type);
		Type = type;
	}

#pragma warning restore CA1019

	/// <summary>
	/// Gets the name of the collection. If <see cref="CollectionAttribute(string)"/> was called,
	/// will return the provided name; if <see cref="CollectionAttribute(Type)"/> was called,
	/// will return a synthetic name for the type.
	/// </summary>
	public string Name { get; }

	/// <inheritdoc/>
	public Type? Type { get; }

	/// <summary>
	/// Gets the collection name that will result for a given type.
	/// </summary>
	/// <param name="type">The collection type</param>
	public static string GetCollectionNameForType(Type type) =>
		string.Format(CultureInfo.InvariantCulture, "Test collection for {0} (id: {1})", Guard.ArgumentNotNull(type).SafeName(), UniqueIDGenerator.ForType(type));
}

/// <summary>
/// Used to declare a specific test collection for a test class. Equivalent to using <see cref="CollectionAttribute"/>
/// with the <see cref="CollectionAttribute(Type)">type-based constructor</see>.
/// </summary>
/// <remarks>
/// .NET Framework does not support generic attributes. Please use the non-generic <see cref="CollectionAttribute"/>
/// when targeting .NET Framework.
/// </remarks>
/// <typeparam name="TCollectionDefinition">The type for the collection definition.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class CollectionAttribute<TCollectionDefinition> : Attribute, ICollectionAttribute
{
	/// <inheritdoc/>
	public string Name { get; } = CollectionAttribute.GetCollectionNameForType(typeof(TCollectionDefinition));

	/// <inheritdoc/>
	public Type? Type { get; } = typeof(TCollectionDefinition);
}
