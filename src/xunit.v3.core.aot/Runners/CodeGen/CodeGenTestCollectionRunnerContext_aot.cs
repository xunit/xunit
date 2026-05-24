using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CodeGenTestAssemblyRunner"/>.
/// </summary>
/// <param name="testCollection">The test collection to be run.</param>
/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
/// <param name="messageBus">The message bus to report run status to.</param>
/// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
/// <param name="parallelismOptions">Options which determine the amount of test parallelization to allow.</param>
/// <param name="parallelizationSemaphore">Semaphore used to limit the number of tests running in parallel.</param>
/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
/// <param name="assemblyFixtureMappings">The mapping manager for assembly fixtures.</param>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestCollectionRunnerContext(
	ICodeGenTestCollection testCollection,
	IReadOnlyCollection<ICodeGenTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	ParallelismOptions parallelismOptions,
	ITestPipelineSemaphore? parallelizationSemaphore,
	CancellationTokenSource cancellationTokenSource,
	FixtureMappingManager assemblyFixtureMappings) :
		CodeGenTestCollectionRunnerBaseContext<ICodeGenTestCollection, ICodeGenTestClass, ICodeGenTestCase>(
			testCollection,
			testCases,
			explicitOption,
			messageBus,
			aggregator,
			parallelismOptions,
			parallelizationSemaphore,
			cancellationTokenSource,
			assemblyFixtureMappings
		)
{
	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunTestClass(
		ICodeGenTestClass testClass,
		IReadOnlyCollection<ICodeGenTestCase> testCases) =>
			CodeGenTestClassRunner.Instance.Run(
				testClass,
				testCases,
				ExplicitOption,
				MessageBus,
				Aggregator.Clone(),
				ParallelismOptions,
				ParallelizationSemaphore,
				CancellationTokenSource,
				CollectionFixtureMappings
			);
}
