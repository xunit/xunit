using System.Collections.Generic;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestCollectionRunner{TContext, TTestCase}"/>.
/// </summary>
public class TestCollectionRunnerContext<TTestCase> : ContextBase
	where TTestCase : _ITestCase
{
	ITestCaseOrderer testCaseOrderer;

	/// <summary>
	/// Initializes a new instance of the <see cref="TestCollectionRunnerContext{TTestCase}"/> class.
	/// </summary>
	public TestCollectionRunnerContext(
		_ITestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource) :
			base(explicitOption, messageBus, aggregator, cancellationTokenSource)
	{
		TestCollection = Guard.ArgumentNotNull(testCollection);
		TestCases = Guard.ArgumentNotNull(testCases);
		this.testCaseOrderer = Guard.ArgumentNotNull(testCaseOrderer);
	}

	/// <summary>
	/// Gets or sets the orderer used to order test cases within the test collection.
	/// </summary>
	public ITestCaseOrderer TestCaseOrderer
	{
		get => testCaseOrderer;
		set => testCaseOrderer = Guard.ArgumentNotNull(value, nameof(TestCaseOrderer));
	}

	/// <summary>
	/// Gets the test cases that belong to the test collection.
	/// </summary>
	public IReadOnlyCollection<TTestCase> TestCases { get; }

	/// <summary>
	/// Gets the test collection that is being executed.
	/// </summary>
	public _ITestCollection TestCollection { get; }
}
