using Xunit.Sdk;
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

	/// <summary>
	/// Determines whether tests in this assembly are run in parallel.
	/// </summary>
	public bool DisableTestParallelization
	{
		get => ParallelismOptions == ParallelismOptions.None;
		set => ParallelismOptions = value ? ParallelismOptions.None : ParallelismOptions;
	}

	/// <summary>
	/// Gets or sets options which determine the amount of parallelization to allow for tests in this assembly by default.
	/// </summary>
	public ParallelismOptions ParallelismOptions { get; set; } = ParallelismOptionsAliases.Default;

	/// <summary>
	/// Determines how many tests can run in parallel with each other. If set to 0, the system will
	/// use <see cref="Environment.ProcessorCount"/>. If set to a negative number, then there will
	/// be no limit to the number of threads.
	/// </summary>
	public int MaxParallelThreads { get; set; }

	/// <summary>
	/// Determines the parallel algorithm used when running tests in parallel.
	/// </summary>
	public ParallelAlgorithm ParallelAlgorithm { get; set; } = ParallelAlgorithm.Conservative;
}

/// <typeparam name="TCollectionFactory">The factory type</typeparam>
partial class CollectionBehaviorAttribute<TCollectionFactory>
	where TCollectionFactory : ICodeGenTestCollectionFactory
{
	/// <summary>
	/// Gets the collection factory type specified by this collection behavior attribute.
	/// </summary>
	public Type? CollectionFactoryType { get; }

	/// <summary>
	/// Determines whether tests in this assembly are run in parallel.
	/// </summary>
	public bool DisableTestParallelization
	{
		get => ParallelismOptions == ParallelismOptions.None;
		set => ParallelismOptions = value ? ParallelismOptions.None : ParallelismOptions;
	}

	/// <summary>
	/// Gets or sets options which determine the amount of parallelization to allow for tests in this assembly by default.
	/// </summary>
	public ParallelismOptions ParallelismOptions { get; set; } = ParallelismOptionsAliases.Default;

	/// <summary>
	/// Determines how many tests can run in parallel with each other. If set to 0, the system will
	/// use <see cref="Environment.ProcessorCount"/>. If set to a negative number, then there will
	/// be no limit to the number of threads.
	/// </summary>
	public int MaxParallelThreads { get; set; }

	/// <summary>
	/// Determines the parallel algorithm used when running tests in parallel.
	/// </summary>
	public ParallelAlgorithm ParallelAlgorithm { get; set; } = ParallelAlgorithm.Conservative;
}
