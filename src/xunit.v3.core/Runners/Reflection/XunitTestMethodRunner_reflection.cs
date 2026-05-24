using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Test method runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestMethodRunner : XunitTestMethodRunnerBase<XunitTestMethodRunnerContext, IXunitTestMethod, IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestMethodRunner"/> class.
	/// </summary>
	protected XunitTestMethodRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitTestMethodRunner"/> class.
	/// </summary>
	public static XunitTestMethodRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test test method.
	/// </summary>
	/// <param name="testMethod">The test method to be run.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="parallelismOptions">Options which determine the amount of test parallelization to allow.</param>
	/// <param name="parallelizationSemaphore">Semaphore used to limit the number of tests running in parallel.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="constructorArguments">The constructor arguments for the test class.</param>
	/// <param name="classFixtureMappings">The fixtures attached to the test class</param>
	public async ValueTask<RunSummary> Run(
		IXunitTestMethod testMethod,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		ParallelismOptions parallelismOptions,
		ITestPipelineSemaphore? parallelizationSemaphore,
		CancellationTokenSource cancellationTokenSource,
		object?[] constructorArguments,
		FixtureMappingManager classFixtureMappings)
	{
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(constructorArguments);

		await using var ctxt = new XunitTestMethodRunnerContext(
			testMethod,
			testCases,
			explicitOption,
			messageBus,
			aggregator,
			parallelismOptions,
			parallelizationSemaphore,
			cancellationTokenSource,
			constructorArguments,
			classFixtureMappings
		);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}
}
