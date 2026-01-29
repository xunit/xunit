using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestCaseRunner{TContext, TTestCase, TTest}"/>.
/// </summary>
/// <param name="testCase">The test case</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
/// <typeparam name="TTest">The type of the test that is generated from the test case. Must
/// derive from <see cref="ITest"/>.</typeparam>
public abstract class TestCaseRunnerContext<TTestCase, TTest>(
	TTestCase testCase,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource) :
		TestCaseRunnerBaseContext<TTestCase>(testCase, explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestCase : class, ITestCase
			where TTest : class, ITest
{
	/// <summary>
	/// Gets the tests for the given test case.
	/// </summary>
	public abstract IReadOnlyCollection<TTest> Tests { get; }
}
