using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Test case runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestCaseRunner :
	XunitTestCaseRunnerBase<XunitTestCaseRunnerContext, IXunitTestCase, IXunitTest>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestCaseRunner"/> class.
	/// </summary>
	protected XunitTestCaseRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitTestCaseRunner"/> class.
	/// </summary>
	public static XunitTestCaseRunner Instance { get; } = new();

	/// <summary>
	/// Please call <see cref="Run(IXunitTestCase, IReadOnlyCollection{IXunitTest}, IMessageBus, ExceptionAggregator, CancellationTokenSource, ParallelMode, ExecutionScheduler, string, string?, ExplicitOption, object?[], FixtureMappingManager)"/>.
	/// This overload is no longer supported and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call the overload which adds parallelMode, scheduler, and methodFixtureMappings. This overload is no longer supported and will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public async ValueTask<RunSummary> Run(
		IXunitTestCase testCase,
		IReadOnlyCollection<IXunitTest> tests,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		string displayName,
		string? skipReason,
		ExplicitOption explicitOption,
		object?[] constructorArguments) =>
			throw new NotSupportedException("Please call the overload which adds parallelMode, scheduler, and methodFixtureMappings. This overload is no longer supported and will be removed in the next major version.");

	/// <summary>
	/// Runs the test case.
	/// </summary>
	/// <remarks>
	/// This entry point is used for both single-test (like <see cref="FactAttribute"/> and individual data
	/// rows for <see cref="TheoryAttribute"/> tests) and multi-test test cases (like <see cref="TheoryAttribute"/>
	/// when pre-enumeration is disable or the theory data was not serializable).
	/// </remarks>
	/// <param name="testCase">The test case that this invocation belongs to.</param>
	/// <param name="tests">The tests for the test case.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="parallelMode">The parallel mode for the test case</param>
	/// <param name="scheduler">The scheduler used for task/test scheduling</param>
	/// <param name="displayName">The display name of the test case.</param>
	/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
	/// <param name="methodFixtureMappings">The fixtures attached to the test method</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	public async ValueTask<RunSummary> Run(
		IXunitTestCase testCase,
		IReadOnlyCollection<IXunitTest> tests,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		ParallelMode parallelMode,
		ExecutionScheduler scheduler,
		string displayName,
		string? skipReason,
		ExplicitOption explicitOption,
		object?[] constructorArguments,
		FixtureMappingManager methodFixtureMappings)
	{
		await using var ctxt = new XunitTestCaseRunnerContext(
			testCase,
			tests,
			explicitOption,
			messageBus,
			aggregator,
			displayName,
			skipReason,
			cancellationTokenSource,
			parallelMode,
			scheduler,
			constructorArguments,
			methodFixtureMappings
		);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}
}
