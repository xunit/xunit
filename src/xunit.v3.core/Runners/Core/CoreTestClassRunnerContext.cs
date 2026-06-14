using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CoreTestClassRunner{TContext, TTestClass, TTestMethod, TTestCase}"/>.
/// </summary>
/// <param name="testClass">The test class</param>
/// <param name="testCases">The test from the test class</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="parallelMode">The parallel mode for the test class</param>
/// <param name="scheduler">The scheduler used for task/test scheduling</param>
/// <typeparam name="TTestClass">The type of the test class used by the test framework. Must
/// derive from <see cref="ICoreTestClass"/>.</typeparam>
/// <typeparam name="TTestMethod">The type of the test method used by the test framework. Must
/// derive from <see cref="ICoreTestMethod"/>.</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ICoreTestCase"/>.</typeparam>
/// <remarks>
/// This class is shared between reflection-based and code generation-based tests.
/// </remarks>
public abstract class CoreTestClassRunnerContext<TTestClass, TTestMethod, TTestCase>(
	TTestClass testClass,
	IReadOnlyCollection<TTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	ParallelMode parallelMode,
	ExecutionScheduler scheduler) :
		TestClassRunnerContext<TTestClass, TTestCase>(testClass, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestClass : class, ICoreTestClass
			where TTestMethod : class, ICoreTestMethod
			where TTestCase : class, ICoreTestCase
{
	/// <summary>
	/// Gets the parallel mode for the test class.
	/// </summary>
	/// <remarks>
	/// Note: This will only return <see cref="ParallelMode.All"/> if that was the parallel mode passed to the constructor,
	/// and the test class has not opted out parallelism; otherwise, it will always return <see cref="ParallelMode.None"/>.
	/// </remarks>
	public ParallelMode ParallelMode { get; } =
		(parallelMode, Guard.ArgumentNotNull(testClass).DisableParallelization) switch
		{
			(ParallelMode.All, false) => ParallelMode.All,
			_ => ParallelMode.None,
		};

	/// <summary>
	/// Gets the scheduler used for task/test scheduling.
	/// </summary>
	public ExecutionScheduler Scheduler { get; } = Guard.ArgumentNotNull(scheduler);

	/// <summary>
	/// Runs a test method from this test class.
	/// </summary>
	/// <param name="testMethod">The test method to run.</param>
	/// <param name="testCases">The test cases that belong to the test method.</param>
	/// <returns></returns>
	public abstract ValueTask<RunSummary> RunTestMethod(TTestMethod testMethod, IReadOnlyCollection<TTestCase> testCases);
}
