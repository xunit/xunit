using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test collection runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestCollectionRunner :
	CodeGenTestCollectionRunnerBase<CodeGenTestCollectionRunnerContext, ICodeGenTestCollection, ICodeGenTestClass, ICodeGenTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CodeGenTestCollectionRunner"/> class.
	/// </summary>
	protected CodeGenTestCollectionRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of <see cref="CodeGenTestCollectionRunner"/>.
	/// </summary>
	public static CodeGenTestCollectionRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test collection.
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
	public async ValueTask<RunSummary> Run(
		ICodeGenTestCollection testCollection,
		IReadOnlyCollection<ICodeGenTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		ParallelismOptions parallelismOptions,
		ITestPipelineSemaphore? parallelizationSemaphore,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager assemblyFixtureMappings)
	{
		Guard.ArgumentNotNull(testCollection);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(cancellationTokenSource);
		Guard.ArgumentNotNull(assemblyFixtureMappings);

		await using var ctxt = new CodeGenTestCollectionRunnerContext(
			testCollection,
			testCases,
			explicitOption,
			messageBus,
			aggregator,
			parallelismOptions,
			parallelizationSemaphore,
			cancellationTokenSource,
			assemblyFixtureMappings
		);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}
}
