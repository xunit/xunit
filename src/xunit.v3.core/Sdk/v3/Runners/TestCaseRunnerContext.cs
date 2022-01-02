using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestCaseRunner{TContext, TTestCase}"/>.
/// </summary>
public class TestCaseRunnerContext<TTestCase>
	where TTestCase : _ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestCaseRunnerContext{TTestCase}"/> class.
	/// </summary>
	/// <param name="testCase"></param>
	/// <param name="messageBus"></param>
	/// <param name="aggregator"></param>
	/// <param name="cancellationTokenSource"></param>
	public TestCaseRunnerContext(
		TTestCase testCase,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		TestCase = Guard.GenericArgumentNotNull(testCase);
		MessageBus = Guard.ArgumentNotNull(messageBus);
		Aggregator = aggregator;
		CancellationTokenSource = Guard.ArgumentNotNull(cancellationTokenSource);
	}

	/// <summary>
	/// Gets the aggregator used for reporting exceptions.
	/// </summary>
	public ExceptionAggregator Aggregator { get; }

	/// <summary>
	/// Gets the cancellation token source used for cancelling test execution.
	/// </summary>
	public CancellationTokenSource CancellationTokenSource { get; }

	/// <summary>
	/// Gets the message bus to send execution engine messages to.
	/// </summary>
	public IMessageBus MessageBus { get; }

	/// <summary>
	/// Gets the test case that is being executed.
	/// </summary>
	public TTestCase TestCase { get; }
}
