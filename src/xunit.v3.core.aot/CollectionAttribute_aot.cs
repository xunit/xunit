namespace Xunit;

/// <summary>
/// Used to declare a specific test collection for a test class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class CollectionAttribute : CollectionAttributeBase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionAttribute" /> class, with the
	/// given collection name.
	/// </summary>
	/// <param name="name">The test collection name.</param>
	public CollectionAttribute(string name) : base(name)
	{ }

	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionAttribute" /> class based on
	/// a collection definition type, with an auto-generated name based on that type. Equivalent
	/// to using <see cref="CollectionAttribute{TCollectionDefinition}"/>.
	/// </summary>
	/// <param name="type">The type representing the collection fixture.</param>
	public CollectionAttribute(Type type) : base(type)
	{ }

	/// <summary>
	/// Gets the collection name that will result for a given type.
	/// </summary>
	/// <param name="type">The collection type</param>
	public static new string GetCollectionNameForType(Type type) =>
		CollectionAttributeBase.GetCollectionNameForType(type);
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
public sealed class CollectionAttribute<TCollectionDefinition>() :
	CollectionAttributeBase(typeof(TCollectionDefinition))
{ }
