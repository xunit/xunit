using Xunit.Sdk;
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

	/// <inheritdoc/>
	[Obsolete($"Use {nameof(ParallelismOptions)} = {nameof(ParallelismOptions.None)} instead. This property will be removed in the next major release.")]
	public bool DisableTestParallelization
	{
		get => ParallelismOptions == ParallelismOptions.None;
		set => ParallelismOptions = value ? ParallelismOptions.None : ParallelismOptions;
	}

	/// <inheritdoc/>
	public ParallelismOptions ParallelismOptions { get; set; } = ParallelismOptionsAliases.Default;

	/// <inheritdoc/>
	public int MaxParallelThreads { get; set; }

	/// <inheritdoc/>
	public ParallelAlgorithm ParallelAlgorithm { get; set; } = ParallelAlgorithm.Conservative;
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

	/// <inheritdoc/>
	[Obsolete($"Use {nameof(ParallelismOptions)} = {nameof(ParallelismOptions.None)} instead. This property will be removed in the next major release.")]
	public bool DisableTestParallelization
	{
		get => ParallelismOptions == ParallelismOptions.None;
		set => ParallelismOptions = value ? ParallelismOptions.None : ParallelismOptions;
	}

	/// <inheritdoc/>
	public ParallelismOptions ParallelismOptions { get; set; } = ParallelismOptionsAliases.Default;

	/// <inheritdoc/>
	public int MaxParallelThreads { get; set; }

	/// <inheritdoc/>
	public ParallelAlgorithm ParallelAlgorithm { get; set; } = ParallelAlgorithm.Conservative;
}
