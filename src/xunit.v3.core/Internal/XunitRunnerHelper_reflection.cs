using Xunit.Sdk;

namespace Xunit.v3;

partial class XunitRunnerHelper
{
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
	/// <returns></returns>
	public static ValueTask<RunSummary> RunXunitTestCase(
		IXunitTestCase testCase,
		IMessageBus messageBus,
		CancellationTokenSource cancellationTokenSource,
		ExceptionAggregator aggregator,
		ExplicitOption explicitOption,
		object?[] constructorArguments) =>
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
					constructorArguments
				),
				cancellationTokenSource
			);
}
