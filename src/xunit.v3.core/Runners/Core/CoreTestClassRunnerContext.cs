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
	CancellationTokenSource cancellationTokenSource) :
		TestClassRunnerContext<TTestClass, TTestCase>(testClass, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestClass : class, ICoreTestClass
			where TTestMethod : class, ICoreTestMethod
			where TTestCase : class, ICoreTestCase
{
	/// <summary>
	/// Runs a test method from this test class.
	/// </summary>
	/// <param name="testMethod">The test method to run.</param>
	/// <param name="testCases">The test cases that belong to the test method.</param>
	/// <returns></returns>
	public abstract ValueTask<RunSummary> RunTestMethod(TTestMethod testMethod, IReadOnlyCollection<TTestCase> testCases);
}
