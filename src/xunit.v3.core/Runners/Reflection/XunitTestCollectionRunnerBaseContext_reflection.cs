using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestCollectionRunnerBaseContext{TTestCollection, TTestClass, TTestCase}"/>.
/// </summary>
/// <param name="testCollection">The test collection</param>
/// <param name="testCases">The test cases from the test collection</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="parallelMode">The parallel mode for the test collection</param>
/// <param name="scheduler">The scheduler used for task/test scheduling</param>
/// <param name="assemblyFixtureMappings">The fixtures associated with the test assembly</param>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public abstract class XunitTestCollectionRunnerBaseContext<TTestCollection, TTestClass, TTestCase>(
	TTestCollection testCollection,
	IReadOnlyCollection<TTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	ParallelMode parallelMode,
	ExecutionScheduler scheduler,
	FixtureMappingManager assemblyFixtureMappings) :
		CoreTestCollectionRunnerContext<TTestCollection, TTestClass, TTestCase>(testCollection, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler)
			where TTestCollection : class, IXunitTestCollection
			where TTestClass : class, IXunitTestClass
			where TTestCase : class, IXunitTestCase
{
	/// <summary>
	/// Please use <see cref="XunitTestCollectionRunnerBaseContext(TTestCollection, IReadOnlyCollection{TTestCase}, ExplicitOption, IMessageBus, ExceptionAggregator, CancellationTokenSource, ParallelMode, ExecutionScheduler, FixtureMappingManager)"/>.
	/// This overload will be removed in the next major version.
	/// </summary>
	[Obsolete("Please use the constructor which removes testCaseOrderer and adds parallelMode and scheduler. This overload is no longer valid and will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected XunitTestCollectionRunnerBaseContext(
		TTestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager assemblyFixtureMappings) :
			this(testCollection, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, ParallelMode.None, ExecutionScheduler.Invalid, assemblyFixtureMappings) =>
				throw new NotSupportedException("Please use the constructor which removes testCaseOrderer and adds parallelMode and scheduler. This overload is no longer valid and will be removed in the next major version.");

	/// <summary>
	/// Gets the mapping manager for collection-level fixtures.
	/// </summary>
	public FixtureMappingManager CollectionFixtureMappings { get; } = new("Collection", Guard.ArgumentNotNull(assemblyFixtureMappings));

	/// <summary>
	/// The optional test case orderer for the test class can be retrieved from <c>TestCollection.TestCaseOrderer</c>.
	/// This property is no longer valid, and will be removed in the next major version.
	/// </summary>
	[Obsolete("The optional test case orderer for the test collection can be retrieved from from TestCollection.TestCaseOrderer. This property is no longer valid, and will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ITestCaseOrderer TestCaseOrderer
	{
		get => TestCollection.TestCaseOrderer ?? TestCollection.TestAssembly.TestCaseOrderer ?? DefaultTestCaseOrderer.Instance;
		set => throw new NotSupportedException("The optional test case orderer for the test collection can be retrieved from from TestCollection.TestCaseOrderer. This property is no longer valid, and will be removed in the next major version.");
	}
}
