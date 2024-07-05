using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestCaseRunner{TContext, TTestCase}"/>.
/// </summary>
/// <param name="testCase">The test case</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
public class TestCaseRunnerContext<TTestCase>(
	TTestCase testCase,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource) :
		ContextBase(explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestCase : class, ITestCase
{
	/// <summary>
	/// Gets the test case that is being executed.
	/// </summary>
	public TTestCase TestCase { get; } = Guard.GenericArgumentNotNull(testCase);
}
