using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CodeGenTestMethodRunnerBase{TContext, TTestMethod, TTestCase}"/>.
/// </summary>
/// <param name="testMethod">The test method</param>
/// <param name="testCases">The test cases from the test method</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="parallelismOptions">Options which determine the amount of test parallelization to allow.</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="classFixtureMappings">The mapping of class fixture types to fixtures.</param>
public abstract class CodeGenTestMethodRunnerBaseContext<TTestMethod, TTestCase>(
	TTestMethod testMethod,
	IReadOnlyCollection<TTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	ParallelismOptions parallelismOptions,
	CancellationTokenSource cancellationTokenSource,
	FixtureMappingManager classFixtureMappings) :
		CoreTestMethodRunnerContext<TTestMethod, TTestCase>(testMethod, testCases, explicitOption, messageBus, aggregator, parallelismOptions, cancellationTokenSource)
			where TTestMethod : class, ICodeGenTestMethod
			where TTestCase : class, ICodeGenTestCase
{
	/// <summary>
	/// Gets the mapping manager for method-level fixtures.
	/// </summary>
	/// <remarks>
	/// There is no mechanism for describing or attaching method-level fixtures at this time, so this currently
	/// returns the mapping manager for the class-level fixtures. If method-level fixtures become a feature in the
	/// future, it is anticipated that this API will return the method-level fixture mapping manager.
	/// </remarks>
	public FixtureMappingManager MethodFixtureMappings { get; } = Guard.ArgumentNotNull(classFixtureMappings);
}
