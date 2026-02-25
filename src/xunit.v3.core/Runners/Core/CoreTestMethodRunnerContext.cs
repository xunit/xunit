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
	CancellationTokenSource cancellationTokenSource) :
		TestMethodRunnerContext<TTestMethod, TTestCase>(testMethod, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestMethod : class, ICoreTestMethod
			where TTestCase : class, ICoreTestCase
{
	/// <summary>
	/// Runs a test case from this test method.
	/// </summary>
	/// <param name="testCase">The test case to be run</param>
	public abstract ValueTask<RunSummary> RunTestCase(TTestCase testCase);
}
