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
/// <param name="constructorArguments">The constructor arguments for the test class</param>
/// <param name="classFixtureMappings">The fixtures attached to the test class</param>
/// <param name="parallelismOptions">Options which determine the amount of test parallelization to allow.</param>
/// <param name="parallelizationSemaphore">Semaphore used to limit the number of tests running in parallel.</param>
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
	object?[] constructorArguments,
	FixtureMappingManager classFixtureMappings,
	ParallelismOptions parallelismOptions,
	SemaphoreSlim? parallelizationSemaphore) :
		CoreTestMethodRunnerContext<TTestMethod, TTestCase>(testMethod, testCases, explicitOption, messageBus, aggregator, parallelismOptions, cancellationTokenSource)
			where TTestMethod : class, IXunitTestMethod
			where TTestCase : class, IXunitTestCase
{
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
			return selfExecutingTestCase.Run(ExplicitOption, MessageBus, ConstructorArguments, MethodFixtureMappings, Aggregator.Clone(), CancellationTokenSource);

		return XunitRunnerHelper.RunXunitTestCase(
			testCase,
			MessageBus,
			CancellationTokenSource,
			Aggregator.Clone(),
			ExplicitOption,
			ConstructorArguments,
			MethodFixtureMappings,
			ParallelismOptions,
			parallelizationSemaphore
		);
	}
}
