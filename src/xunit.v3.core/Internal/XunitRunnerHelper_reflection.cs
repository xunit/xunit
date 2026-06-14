using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

partial class XunitRunnerHelper
{
	/// <summary>
	/// Please call <see cref="RunXunitTestCase(IXunitTestCase, IMessageBus, CancellationTokenSource, ParallelMode, ExecutionScheduler, ExceptionAggregator, ExplicitOption, object?[], FixtureMappingManager)"/>.
	/// This overload is not supported, and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call the overload that adds parallelMode, scheduler, and methodFixtureMappings. This overload is not supported, and will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static ValueTask<RunSummary> RunXunitTestCase(
		IXunitTestCase testCase,
		IMessageBus messageBus,
		CancellationTokenSource cancellationTokenSource,
		ExceptionAggregator aggregator,
		ExplicitOption explicitOption,
		object?[] constructorArguments) =>
			throw new NotSupportedException("Please call the overload that adds parallelMode, scheduler, and methodFixtureMappings. This overload is not supported, and will be removed in the next major version.");

	/// <summary>
	/// Runs a single test case (which implements <see cref="IXunitTestCase"/>) using
	/// the <see cref="XunitTestCaseRunner"/> after enumerating all tests.
	/// </summary>
	/// <param name="testCase">The test case to run</param>
	/// <param name="messageBus">The message bus to send the messages to</param>
	/// <param name="cancellationTokenSource">The cancellation token source to cancel if requested</param>
	/// <param name="parallelMode">The parallel mode for the test case</param>
	/// <param name="scheduler">The scheduler used for task/test scheduling</param>
	/// <param name="aggregator">The exception aggregator to record exceptions to</param>
	/// <param name="explicitOption">A flag to indicate which types of tests to run (non-explicit, explicit, or both)</param>
	/// <param name="constructorArguments">The arguments to pass to the test class constructor</param>
	/// <param name="methodFixtureMappings">The fixtures attached to the test method</param>
	public static ValueTask<RunSummary> RunXunitTestCase(
		IXunitTestCase testCase,
		IMessageBus messageBus,
		CancellationTokenSource cancellationTokenSource,
		ParallelMode parallelMode,
		ExecutionScheduler scheduler,
		ExceptionAggregator aggregator,
		ExplicitOption explicitOption,
		object?[] constructorArguments,
		FixtureMappingManager methodFixtureMappings) =>
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
					parallelMode,
					scheduler,
					testCase.TestCaseDisplayName,
					testCase.SkipReason,
					explicitOption,
					constructorArguments,
					methodFixtureMappings
				),
				cancellationTokenSource
			);
}
