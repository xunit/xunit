using System.ComponentModel;
using System.Runtime.CompilerServices;
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
	FixtureMappingManager assemblyFixtureMappings) :
		CoreTestCollectionRunnerContext<TTestCollection, TTestClass, TTestCase>(testCollection, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTestCollection : class, IXunitTestCollection
			where TTestClass : class, IXunitTestClass
			where TTestCase : class, IXunitTestCase
{
	/// <summary>
	/// Please use <see cref="XunitTestCollectionRunnerBaseContext(TTestCollection, IReadOnlyCollection{TTestCase}, ExplicitOption, IMessageBus, ExceptionAggregator, CancellationTokenSource, FixtureMappingManager)"/>.
	/// This overload will be removed in the next major version.
	/// </summary>
	[Obsolete("Please use the constructor which accepts testClassOrderer and testMethodOrderer. This overload will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[OverloadResolutionPriority(-1)]
	protected XunitTestCollectionRunnerBaseContext(
		TTestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager assemblyFixtureMappings) :
			this(
				testCollection,
				testCases,
				explicitOption,
				messageBus,
				aggregator,
				cancellationTokenSource,
				assemblyFixtureMappings
			)
	{ }

	/// <summary>
	/// Gets the mapping manager for collection-level fixtures.
	/// </summary>
	public FixtureMappingManager CollectionFixtureMappings { get; } = new("Collection", Guard.ArgumentNotNull(assemblyFixtureMappings));
}
