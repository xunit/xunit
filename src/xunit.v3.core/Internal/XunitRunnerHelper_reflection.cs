using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

partial class XunitRunnerHelper
{
	/// <summary>
	/// Please call <see cref="RunXunitTestCase(IXunitTestCase, IMessageBus, CancellationTokenSource, ExceptionAggregator, ExplicitOption, object?[], FixtureMappingManager, ParallelismOptions, ITestPipelineSemaphore?)"/>.
	/// This overload is not supported, and will be removed from the next major version.
	/// </summary>
	[Obsolete("Please call the overload that accepts methodFixtureMappings. This overload is not supported, and will be removed from the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static ValueTask<RunSummary> RunXunitTestCase(
		IXunitTestCase testCase,
		IMessageBus messageBus,
		CancellationTokenSource cancellationTokenSource,
		ExceptionAggregator aggregator,
		ExplicitOption explicitOption,
		object?[] constructorArguments) =>
			throw new PlatformNotSupportedException("Please call the overload that accepts methodFixtureMappings. This overload is not supported, and will be removed from the next major version.");

	/// <summary>
	/// Runs a single test case (which implements <see cref="IXunitTestCase"/>) using
	/// the <see cref="XunitTestCaseRunner"/> after enumerating all tests.
	/// </summary>
	/// <param name="testCase">The test case to run</param>
	/// <param name="messageBus">The message bus to send the messages to</param>
	/// <param name="cancellationTokenSource">The cancellation token source to cancel if requested</param>
	/// <param name="aggregator">The exception aggregator to record exceptions to</param>
	/// <param name="explicitOption">A flag to indicate which types of tests to run (non-explicit, explicit, or both)</param>
	/// <param name="constructorArguments">The arguments to pass to the test class constructor</param>
	/// <param name="methodFixtureMappings">The fixtures attached to the test method</param>
	/// <param name="parallelismOptions">Options which determine the amount of test parallelization to allow.</param>
	/// <param name="parallelizationSemaphore">Semaphore used to limit the number of tests running in parallel.</param>
	public static ValueTask<RunSummary> RunXunitTestCase(
		IXunitTestCase testCase,
		IMessageBus messageBus,
		CancellationTokenSource cancellationTokenSource,
		ExceptionAggregator aggregator,
		ExplicitOption explicitOption,
		object?[] constructorArguments,
		FixtureMappingManager methodFixtureMappings,
		ParallelismOptions parallelismOptions = ParallelismOptionsAliases.Default,
		ITestPipelineSemaphore? parallelizationSemaphore = null) =>
			RunCoreTestCase(
				Guard.ArgumentNotNull(testCase),
				messageBus,
				aggregator,
				testCase.CreateTests,
				tests => XunitTestCaseRunner.Instance.Run(
					testCase,
					tests,
					messageBus,
					aggregator,
					cancellationTokenSource,
					testCase.TestCaseDisplayName,
					testCase.SkipReason,
					explicitOption,
					constructorArguments,
					methodFixtureMappings,
					parallelismOptions,
					parallelizationSemaphore
				),
				cancellationTokenSource
			);
}
