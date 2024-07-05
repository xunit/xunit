using System.Collections.Generic;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestCollectionRunner{TContext, TTestCollection, TTestClass, TTestCase}"/>.
/// </summary>
/// <param name="testCollection">The test collection</param>
/// <param name="testCases">The test cases from the test collection</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <typeparam name="TTestCollection">The type of the test collection used by the test framework.
/// Must derive from <see cref="ITestCollection"/>.</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
public class TestCollectionRunnerContext<TTestCollection, TTestCase>(
	TTestCollection testCollection,
	IReadOnlyCollection<TTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource) :
		ContextBase(explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestCollection : class, ITestCollection
			where TTestCase : class, ITestCase
{
	/// <summary>
	/// Gets the test cases that belong to the test collection.
	/// </summary>
	public IReadOnlyCollection<TTestCase> TestCases { get; protected set; } = Guard.ArgumentNotNull(testCases);

	/// <summary>
	/// Gets the test collection that is being executed.
	/// </summary>
	public TTestCollection TestCollection { get; } = Guard.ArgumentNotNull(testCollection);
}
