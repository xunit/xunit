using Xunit;
using Xunit.Sdk;
using Xunit.v3;

partial class TestData
{
	public static CollectionBehaviorAttribute CollectionBehaviorAttribute(
		bool disableTestParallelization = false,
		ParallelismOptions parallelismOptions = ParallelismOptionsAliases.Default,
		int maxParallelThreads = 0,
		ParallelAlgorithm parallelAlgorithm = ParallelAlgorithm.Conservative) =>
			new()
			{
				ParallelismOptions = parallelismOptions,
				DisableTestParallelization = disableTestParallelization,
				MaxParallelThreads = maxParallelThreads,
				ParallelAlgorithm = parallelAlgorithm
			};

	public static CollectionBehaviorAttribute CollectionBehaviorAttribute(
		CollectionBehavior collectionBehavior,
		bool disableTestParallelization = false,
		ParallelismOptions parallelismOptions = ParallelismOptionsAliases.Default,
		int maxParallelThreads = 0,
		ParallelAlgorithm parallelAlgorithm = ParallelAlgorithm.Conservative) =>
			new(collectionBehavior)
			{
				ParallelismOptions = parallelismOptions,
				DisableTestParallelization = disableTestParallelization,
				MaxParallelThreads = maxParallelThreads,
				ParallelAlgorithm = parallelAlgorithm
			};

	public static CollectionBehaviorAttribute CollectionBehaviorAttribute(
		Type collectionFactoryType,
		bool disableTestParallelization = false,
		int maxParallelThreads = 0,
		ParallelAlgorithm parallelAlgorithm = ParallelAlgorithm.Conservative) =>
			new(collectionFactoryType)
			{
				DisableTestParallelization = disableTestParallelization,
				MaxParallelThreads = maxParallelThreads,
				ParallelAlgorithm = parallelAlgorithm
			};

	public static CollectionBehaviorAttribute<TCollectionFactory> CollectionBehaviorAttribute<TCollectionFactory>(
		bool disableTestParallelization = false,
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
					DisableTestParallelization = disableTestParallelization,
					ParallelismOptions = parallelismOptions,
					MaxParallelThreads = maxParallelThreads,
					ParallelAlgorithm = parallelAlgorithm
				};
}
