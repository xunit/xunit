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
	/// Initializes a new instance of the <see cref="CollectionAttribute" /> class.
	/// </summary>
	/// <param name="name">The test collection name.</param>
	public CollectionAttribute(string name)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionAttribute" /> class.
	/// </summary>
	/// <param name="type">The type representing the collection fixture.</param>
	public CollectionAttribute(Type type)
	{ }
}

/// <summary>
/// Used to declare a specific test collection for a test class.
/// </summary>
/// <typeparam name="TCollectionDefinition">The class to operate as a collection fixture. This class must implement <see cref="ICollectionFixture{TFixture}"/>.</typeparam>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class CollectionAttribute<TCollectionDefinition> : Attribute
{ }
