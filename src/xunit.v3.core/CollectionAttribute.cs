#pragma warning disable CA1019 // The attribute arguments are always read via reflection

using System;

namespace Xunit;

/// <summary>
/// Used to declare a specific test collection for a test class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class CollectionAttribute : Attribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionAttribute" /> class, with the
	/// given collection name.
	/// </summary>
	/// <param name="name">The test collection name.</param>
	public CollectionAttribute(string name)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionAttribute" /> class based on
	/// a collection definition type, with an auto-generated name based on that type. Equivalent
	/// to using <see cref="CollectionAttribute{TCollectionDefinition}"/>.
	/// </summary>
	/// <param name="type">The type representing the collection fixture.</param>
	public CollectionAttribute(Type type)
	{ }
}

#if !NETFRAMEWORK

/// <summary>
/// Used to declare a specific test collection for a test class. Equivalent to using <see cref="CollectionAttribute"/>
/// with the <see cref="CollectionAttribute(Type)">type-based constructor</see>.
/// </summary>
/// <typeparam name="TCollectionDefinition">The type for the collection definition.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class CollectionAttribute<TCollectionDefinition> : Attribute
{ }

#endif
