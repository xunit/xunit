using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CoreTestCollectionRunner{TContext, TTestCollection, TTestClass, TTestCase}"/>.
/// </summary>
/// <param name="testCollection">The test collection</param>
/// <param name="testCases">The test cases from the test collection</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="parallelMode">The parallel mode for the test collection</param>
/// <param name="scheduler">The scheduler used for task/test scheduling</param>
/// <typeparam name="TTestCollection">The type of the test collection used by the test framework. Must
/// derive from <see cref="ICoreTestCase"/>.</typeparam>
/// <typeparam name="TTestClass">The type of the test class used by the test framework. Must
/// derive from <see cref="ICoreTestClass"/>.</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ICoreTestCase"/>.</typeparam>
/// <remarks>
/// This class is shared between reflection-based and code generation-based tests.
/// </remarks>
public abstract class CoreTestCollectionRunnerContext<TTestCollection, TTestClass, TTestCase>(
	TTestCollection testCollection,
	IReadOnlyCollection<TTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	ParallelMode parallelMode,
	ExecutionScheduler scheduler) :
		TestCollectionRunnerContext<TTestCollection, TTestCase>(testCollection, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestCollection : class, ICoreTestCollection
			where TTestClass : class, ICoreTestClass
			where TTestCase : class, ICoreTestCase
{
	/// <summary>
	/// Gets the parallel mode for the test collection.
	/// </summary>
	/// <remarks>
	/// Note: This will only return <see cref="ParallelMode.All"/> if that was the parallel mode passed to the constructor,
	/// and the test collection has not opted out parallelism; otherwise, it will always return <see cref="ParallelMode.None"/>.
	/// </remarks>
	public ParallelMode ParallelMode { get; } =
		(parallelMode, Guard.ArgumentNotNull(testCollection).DisableParallelization) switch
		{
			(ParallelMode.All, false) => ParallelMode.All,
			_ => ParallelMode.None,
		};

	/// <summary>
	/// Gets the scheduler used for task/test scheduling.
	/// </summary>
	public ExecutionScheduler Scheduler { get; } = Guard.ArgumentNotNull(scheduler);

	/// <summary>
	/// Runs the test class.
	/// </summary>
	/// <param name="testClass">The test class to run</param>
	/// <param name="testCases">The test cases in the test class</param>
	public abstract ValueTask<RunSummary> RunTestClass(
		TTestClass testClass,
		IReadOnlyCollection<TTestCase> testCases);
}
