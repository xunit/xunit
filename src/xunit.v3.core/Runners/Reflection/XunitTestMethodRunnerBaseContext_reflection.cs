using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestMethodRunnerBase{TContext, TTestMethod, TTestCase}"/>.
/// </summary>
/// <param name="testMethod">The test method</param>
/// <param name="testCases">The test cases from the test method</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="parallelMode">The parallel mode for the test method</param>
/// <param name="scheduler">The scheduler used for task/test scheduling</param>
/// <param name="constructorArguments">The constructor arguments for the test class</param>
/// <param name="classFixtureMappings">The fixtures attached to the test class</param>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestMethodRunnerBaseContext<TTestMethod, TTestCase>(
	TTestMethod testMethod,
	IReadOnlyCollection<TTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	ParallelMode parallelMode,
	ExecutionScheduler scheduler,
	object?[] constructorArguments,
	FixtureMappingManager classFixtureMappings) :
		CoreTestMethodRunnerContext<TTestMethod, TTestCase>(testMethod, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler)
			where TTestMethod : class, IXunitTestMethod
			where TTestCase : class, IXunitTestCase
{
	/// <summary>
	/// Please call <see cref="XunitTestMethodRunnerBaseContext(TTestMethod, IReadOnlyCollection{TTestCase}, ExplicitOption, IMessageBus, ExceptionAggregator, CancellationTokenSource, ParallelMode, ExecutionScheduler, object?[], FixtureMappingManager)"/>.
	/// This overload is no longer valid and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call the constructor which adds parallelMode, scheduler, and classFixtureMappings. This overload is no longer valid and will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public XunitTestMethodRunnerBaseContext(
		TTestMethod testMethod,
		IReadOnlyCollection<TTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		object?[] constructorArguments) :
			this(testMethod, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, ParallelMode.None, ExecutionScheduler.Invalid, constructorArguments, FixtureMappingManager.Empty) =>
				throw new NotSupportedException("Please call the constructor which adds parallelMode, scheduler, and classFixtureMappings. This overload is no longer valid and will be removed in the next major version.");

	/// <summary>
	/// Gets the arguments to send to the test class constructor.
	/// </summary>
	public object?[] ConstructorArguments { get; } = Guard.ArgumentNotNull(constructorArguments);

	/// <summary>
	/// Gets the mapping manager for method-level fixtures.
	/// </summary>
	/// <remarks>
	/// There is no mechanism for describing or attaching method-level fixtures at this time, so this currently
	/// returns the mapping manager for the class-level fixtures. If method-level fixtures become a feature in the
	/// future, it is anticipated that this API will return the method-level fixture mapping manager.
	/// </remarks>
	public FixtureMappingManager MethodFixtureMappings { get; } = Guard.ArgumentNotNull(classFixtureMappings);

	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunTestCase(TTestCase testCase)
	{
		if (testCase is ISelfExecutingXunitTestCase selfExecutingTestCase)
			return selfExecutingTestCase.Run(ExplicitOption, MessageBus, ConstructorArguments, Aggregator.Clone(), CancellationTokenSource, ParallelMode, Scheduler, MethodFixtureMappings);

		return XunitRunnerHelper.RunXunitTestCase(
			testCase,
			MessageBus,
			CancellationTokenSource,
			ParallelMode,
			Scheduler,
			Aggregator.Clone(),
			ExplicitOption,
			ConstructorArguments,
			MethodFixtureMappings
		);
	}
}
