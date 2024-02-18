#pragma warning disable CA1019 // The attribute arguments are always read via reflection

using System;

namespace Xunit;

/// <summary>
/// Used to declare a test collection container class. The container class gives
/// developers a place to attach interfaces like <see cref="IClassFixture{T}"/> and
/// <see cref="ICollectionFixture{T}"/> that will be applied to all tests classes
/// that are members of the test collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CollectionDefinitionAttribute : Attribute
{
#if NETFRAMEWORK
	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionDefinitionAttribute" /> class.
	/// Use this constructor when collection referenced by test classes use
	/// <see cref="CollectionAttribute(Type)"/>.
	/// </summary>
#else
	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionDefinitionAttribute" /> class.
	/// Use this constructor when collection referenced by test classes use the generic
	/// <see cref="CollectionAttribute{TCollectionDefinition}"/> attribute or refer to the
	/// fixture class using <see cref="CollectionAttribute(Type)"/>.
	/// </summary>
#endif
	public CollectionDefinitionAttribute()
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionDefinitionAttribute" /> class.
	/// Use this constructor when collection referenced by test classes use
	/// <see cref="CollectionAttribute(string)"/>.
	/// </summary>
	/// <param name="name">The test collection name.</param>
	public CollectionDefinitionAttribute(string name)
	{ }

	/// <summary>
	/// Determines whether tests in this collection runs in parallel with any other collections.
	/// </summary>
	public bool DisableParallelization { get; set; }
}
