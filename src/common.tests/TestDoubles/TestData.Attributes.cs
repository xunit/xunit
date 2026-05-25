using Xunit;
using Xunit.Sdk;
using Xunit.v3;

partial class TestData
{
	public static CollectionBehaviorAttribute CollectionBehaviorAttribute(
		ParallelismOptions parallelismOptions = ParallelismOptionsAliases.Default,
		int maxParallelThreads = 0,
		ParallelAlgorithm parallelAlgorithm = ParallelAlgorithm.Conservative) =>
			new()
			{
				ParallelismOptions = parallelismOptions,
				MaxParallelThreads = maxParallelThreads,
				ParallelAlgorithm = parallelAlgorithm
			};

	public static CollectionBehaviorAttribute CollectionBehaviorAttribute(
		CollectionBehavior collectionBehavior,
		ParallelismOptions parallelismOptions = ParallelismOptionsAliases.Default,
		int maxParallelThreads = 0,
		ParallelAlgorithm parallelAlgorithm = ParallelAlgorithm.Conservative) =>
			new(collectionBehavior)
			{
				ParallelismOptions = parallelismOptions,
				MaxParallelThreads = maxParallelThreads,
				ParallelAlgorithm = parallelAlgorithm
			};

	public static CollectionBehaviorAttribute CollectionBehaviorAttribute(
		Type collectionFactoryType,
		int maxParallelThreads = 0,
		ParallelismOptions parallelismOptions = ParallelismOptionsAliases.Default,
		ParallelAlgorithm parallelAlgorithm = ParallelAlgorithm.Conservative) =>
			new(collectionFactoryType)
			{
				MaxParallelThreads = maxParallelThreads,
				ParallelAlgorithm = parallelAlgorithm,
				ParallelismOptions = parallelismOptions
			};

	public static CollectionBehaviorAttribute<TCollectionFactory> CollectionBehaviorAttribute<TCollectionFactory>(
		ParallelismOptions parallelismOptions = ParallelismOptionsAliases.Default,
		int maxParallelThreads = 0,
		ParallelAlgorithm parallelAlgorithm = ParallelAlgorithm.Conservative)
#if XUNIT_AOT
			where TCollectionFactory : ICodeGenTestCollectionFactory =>
#else
			where TCollectionFactory : IXunitTestCollectionFactory =>
#endif
				new()
				{
					ParallelismOptions = parallelismOptions,
					MaxParallelThreads = maxParallelThreads,
					ParallelAlgorithm = parallelAlgorithm
				};
}
