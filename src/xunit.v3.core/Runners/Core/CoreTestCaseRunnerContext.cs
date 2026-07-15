using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CoreTestCaseRunner{TContext, TTestCase, TTest}"/>.
/// </summary>
/// <param name="testCase">The test case</param>
/// <param name="tests">The tests for the test case</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="displayName">The display name of the test case</param>
/// <param name="skipReason">The skip reason, if the test case is being skipped</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="parallelMode">The parallel mode for the test case</param>
/// <param name="scheduler">The scheduler used for task/test scheduling</param>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ICoreTestCase"/>.</typeparam>
/// <typeparam name="TTest">The type of the test used by the test framework. Must
/// derive from <see cref="ICoreTest"/>.</typeparam>
/// <remarks>
/// This class is shared between reflection-based and code generation-based tests.
/// </remarks>
public abstract class CoreTestCaseRunnerContext<TTestCase, TTest>(
	TTestCase testCase,
	IReadOnlyCollection<TTest> tests,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	string displayName,
	string? skipReason,
	CancellationTokenSource cancellationTokenSource,
	ParallelMode parallelMode,
	ExecutionScheduler scheduler) :
		TestCaseRunnerContext<TTestCase, TTest>(testCase, explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestCase : class, ICoreTestCase
			where TTest : class, ICoreTest
{
	/// <summary>
	/// Gets the display name of the test case.
	/// </summary>
	public string DisplayName { get; } = Guard.ArgumentNotNullOrEmpty(displayName);

	/// <summary>
	/// Gets the parallel mode for the test case.
	/// </summary>
	/// <remarks>
	/// Note: This will only return <see cref="ParallelMode.All"/> if that was the parallel mode passed to the constructor,
	/// and the test case has not opted out parallelism; otherwise, it will always return <see cref="ParallelMode.None"/>.
	/// </remarks>
	public ParallelMode ParallelMode { get; } =
		(parallelMode, Guard.ArgumentNotNull(testCase).DisableParallelization) switch
		{
			(ParallelMode.All, false) => ParallelMode.All,
			_ => ParallelMode.None,
		};

	/// <summary>
	/// Gets the scheduler used for task/test scheduling.
	/// </summary>
	public ExecutionScheduler Scheduler { get; } = Guard.ArgumentNotNull(scheduler);

#if XUNIT_AOT
	/// <summary>
	/// Gets the statically specified skip reason for the test. Note that this only covers values
	/// passed via <see cref="FactAttribute"/>.Skip, and not dynamically skipped tests.
	/// </summary>
#else
	/// <summary>
	/// Gets the statically specified skip reason for the test. Note that this only covers values
	/// passed via <see cref="IFactAttribute.Skip"/>, and not dynamically skipped tests.
	/// </summary>
#endif
	public string? SkipReason { get; } = skipReason;

	/// <inheritdoc/>
	public override IReadOnlyCollection<TTest> Tests { get; } = Guard.ArgumentNotNull(tests);

	/// <summary>
	/// Runs a test from this test case.
	/// </summary>
	/// <param name="test">The test to be run</param>
	public abstract ValueTask<RunSummary> RunTest(TTest test);
}
