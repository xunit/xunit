using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestMethodRunner"/>.
/// </summary>
/// <param name="testMethod">The test method</param>
/// <param name="testCases">The test cases from the test method</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="parallelismOptions">Options which determine the amount of test parallelization to allow.</param>
/// <param name="parallelizationSemaphore">Semaphore used to limit the number of tests running in parallel.</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="constructorArguments">The constructor arguments for the test class</param>
/// <param name="classFixtureMappings">The fixtures attached to the test class</param>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestMethodRunnerContext(
	IXunitTestMethod testMethod,
	IReadOnlyCollection<IXunitTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	ParallelismOptions parallelismOptions,
	ITestPipelineSemaphore? parallelizationSemaphore,
	CancellationTokenSource cancellationTokenSource,
	object?[] constructorArguments,
	FixtureMappingManager classFixtureMappings) :
		XunitTestMethodRunnerBaseContext<IXunitTestMethod, IXunitTestCase>(
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
		)
{ }
