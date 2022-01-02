using System.Collections.Generic;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestClassRunner{TContext, TTestCase}"/>.
/// </summary>
public class TestClassRunnerContext<TTestCase>
	where TTestCase : _ITestCase
{
	ITestCaseOrderer testCaseOrderer;

	/// <summary>
	/// Initializes a new instancew of the <see cref="TestClassRunnerContext{TTestCase}"/> class.
	/// </summary>
	public TestClassRunnerContext(
		_ITestClass? testClass,
		_IReflectionTypeInfo? @class,
		IReadOnlyCollection<TTestCase> testCases,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		TestClass = testClass;
		Class = @class;
		TestCases = Guard.ArgumentNotNull(testCases);
		MessageBus = Guard.ArgumentNotNull(messageBus);
		this.testCaseOrderer = Guard.ArgumentNotNull(testCaseOrderer);
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
	/// Gets the type that maps to this test class.
	/// </summary>
	public _IReflectionTypeInfo? Class { get; }

	/// <summary>
	/// Gets the message bus to send execution engine messages to.
	/// </summary>
	public IMessageBus MessageBus { get; }

	/// <summary>
	/// Gets or sets the orderer used to order the test cases.
	/// </summary>
	public ITestCaseOrderer TestCaseOrderer
	{
		get => testCaseOrderer;
		set => testCaseOrderer = Guard.ArgumentNotNull(value, nameof(TestCaseOrderer));
	}

	/// <summary>
	/// Gets the test cases associated with the test class.
	/// </summary>
	public IReadOnlyCollection<TTestCase> TestCases { get; }

	/// <summary>
	/// Gets the test class that is being executed.
	/// </summary>
	public _ITestClass? TestClass { get; }
}
