using System.Collections.Generic;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestMethodRunner{TContext, TTestCase, TTestMethod}"/>.
/// </summary>
/// <param name="testMethod">The test method</param>
/// <param name="testCases">The test cases from the test method</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <typeparam name="TTestMethod">The type of the test method used by the test framework.
/// Must derive from <see cref="ITestMethod"/>.</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
public class TestMethodRunnerContext<TTestMethod, TTestCase>(
	TTestMethod testMethod,
	IReadOnlyCollection<TTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource) :
		ContextBase(explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestMethod : class, ITestMethod
			where TTestCase : class, ITestCase
{
	/// <summary>
	/// Gets the test cases that are derived from this test method.
	/// </summary>
	public IReadOnlyCollection<TTestCase> TestCases { get; } = Guard.ArgumentNotNull(testCases);

	/// <summary>
	/// Gets the test method that is being executed.
	/// </summary>
	public TTestMethod TestMethod { get; } = Guard.ArgumentNotNull(testMethod);
}
