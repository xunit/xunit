using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CoreTestMethodRunner{TContext, TTestMethod, TTestCase}"/>.
/// </summary>
/// <param name="testMethod">The test method</param>
/// <param name="testCases">The test cases from the test method</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="parallelMode">The parallel mode for the test method</param>
/// <param name="scheduler">The scheduler used for task/test scheduling</param>
/// <typeparam name="TTestMethod">The type of the test method used by the test framework. Must
/// derive from <see cref="ICoreTestMethod"/>.</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ICoreTestCase"/>.</typeparam>
/// <remarks>
/// This class is shared between reflection-based and code generation-based tests.
/// </remarks>
public abstract class CoreTestMethodRunnerContext<TTestMethod, TTestCase>(
	TTestMethod testMethod,
	IReadOnlyCollection<TTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	ParallelMode parallelMode,
	ExecutionScheduler scheduler) :
		TestMethodRunnerContext<TTestMethod, TTestCase>(testMethod, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestMethod : class, ICoreTestMethod
			where TTestCase : class, ICoreTestCase
{
	/// <summary>
	/// Gets the parallel mode for the test method.
	/// </summary>
	/// <remarks>
	/// Note: This will only return <see cref="ParallelMode.All"/> if that was the parallel mode passed to the constructor,
	/// and the test method has not opted out parallelism; otherwise, it will always return <see cref="ParallelMode.None"/>.
	/// </remarks>
	public ParallelMode ParallelMode { get; } =
		(parallelMode, Guard.ArgumentNotNull(testMethod).DisableParallelization) switch
		{
			(ParallelMode.All, false) => ParallelMode.All,
			_ => ParallelMode.None,
		};

	/// <summary>
	/// Gets the scheduler used for task/test scheduling.
	/// </summary>
	public ExecutionScheduler Scheduler { get; } = Guard.ArgumentNotNull(scheduler);

	/// <summary>
	/// Runs a test case from this test method.
	/// </summary>
	/// <param name="testCase">The test case to be run</param>
	public abstract ValueTask<RunSummary> RunTestCase(TTestCase testCase);
}
