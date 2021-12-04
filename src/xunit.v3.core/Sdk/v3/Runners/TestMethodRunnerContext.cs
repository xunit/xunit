using System.Collections.Generic;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestMethodRunner{TContext, TTestCase}"/>.
/// </summary>
public class TestMethodRunnerContext<TTestCase>
	where TTestCase : _ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestMethodRunnerContext{TTestCase}"/> class.
	/// </summary>
	// TODO: Why are method and class nullable here?
	public TestMethodRunnerContext(
		_ITestMethod? testMethod,
		_IReflectionTypeInfo? @class,
		_IReflectionMethodInfo? method,
		IReadOnlyCollection<TTestCase> testCases,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		TestMethod = testMethod;
		Class = @class;
		Method = method;
		TestCases = Guard.ArgumentNotNull(testCases);
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
	/// Gets the class that this test method is associated with.
	/// </summary>
	public _IReflectionTypeInfo? Class { get; }

	/// <summary>
	/// Gets the message bus to send execution engine messages to.
	/// </summary>
	public IMessageBus MessageBus { get; }

	/// <summary>
	/// Gets the method that this test method derives from.
	/// </summary>
	public _IReflectionMethodInfo? Method { get; }

	/// <summary>
	/// Gets the test cases that are derived from this test method.
	/// </summary>
	public IReadOnlyCollection<TTestCase> TestCases { get; }

	/// <summary>
	/// Gets the test method.
	/// </summary>
	public _ITestMethod? TestMethod { get; }
}
