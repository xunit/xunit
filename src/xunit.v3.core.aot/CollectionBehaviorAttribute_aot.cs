using Xunit.v3;

namespace Xunit;

sealed partial class CollectionBehaviorAttribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionBehaviorAttribute" /> class
	/// with the given custom collection behavior.
	/// </summary>
	/// <param name="collectionFactoryType">The factory type (must implement <see cref="ICodeGenTestCollectionFactory"/>)</param>
	public partial CollectionBehaviorAttribute(Type collectionFactoryType);

	/// <summary>
	/// Gets the collection factory type specified by this collection behavior attribute.
	/// </summary>
	public Type? CollectionFactoryType { get; }
}

/// <typeparam name="TCollectionFactory">The factory type</typeparam>
partial class CollectionBehaviorAttribute<TCollectionFactory>
	where TCollectionFactory : ICodeGenTestCollectionFactory
{
	/// <summary>
	/// Gets the collection factory type specified by this collection behavior attribute.
	/// </summary>
	public Type? CollectionFactoryType { get; }
}
