using Xunit.v3;

namespace Xunit;

partial class CollectionBehaviorAttribute : ICollectionBehaviorAttribute
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CollectionBehaviorAttribute" /> class
	/// with the given custom collection behavior.
	/// </summary>
	/// <param name="collectionFactoryType">The factory type (must implement <see cref="IXunitTestCollectionFactory"/>)</param>
	public partial CollectionBehaviorAttribute(Type collectionFactoryType);

	/// <inheritdoc/>
	public Type? CollectionFactoryType { get; }
}

/// <typeparam name="TCollectionFactory">The factory type</typeparam>
/// <remarks>
/// .NET Framework does not support generic attributes. Please use the non-generic <see cref="CollectionBehaviorAttribute"/>
/// when targeting .NET Framework.
/// </remarks>
partial class CollectionBehaviorAttribute<TCollectionFactory> : ICollectionBehaviorAttribute
	where TCollectionFactory : IXunitTestCollectionFactory
{
	/// <inheritdoc/>
	public Type? CollectionFactoryType => typeof(TCollectionFactory);
}
