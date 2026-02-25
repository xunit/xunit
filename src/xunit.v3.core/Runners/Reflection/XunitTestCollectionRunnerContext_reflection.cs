using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestCollectionRunner"/>.
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
public class XunitTestCollectionRunnerContext(
	IXunitTestCollection testCollection,
	IReadOnlyCollection<IXunitTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	FixtureMappingManager assemblyFixtureMappings) :
		XunitTestCollectionRunnerBaseContext<IXunitTestCollection, IXunitTestClass, IXunitTestCase>(testCollection, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, assemblyFixtureMappings)
{
	/// <summary>
	/// Please use <see cref="XunitTestCollectionRunnerContext(IXunitTestCollection, IReadOnlyCollection{IXunitTestCase}, ExplicitOption, IMessageBus, ExceptionAggregator, CancellationTokenSource, FixtureMappingManager)"/>.
	/// This overload will be removed in the next major version.
	/// </summary>
	[Obsolete("Please use the constructor which accepts testClassOrderer and testMethodOrderer. This overload will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[OverloadResolutionPriority(-1)]
	public XunitTestCollectionRunnerContext(
		IXunitTestCollection testCollection,
		IReadOnlyCollection<IXunitTestCase> testCases,
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

	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunTestClass(
		IXunitTestClass testClass,
		IReadOnlyCollection<IXunitTestCase> testCases) =>
			XunitTestClassRunner.Instance.Run(
				Guard.ArgumentNotNull(testClass),
				testCases,
				ExplicitOption,
				MessageBus,
				Aggregator.Clone(),
				CancellationTokenSource,
				CollectionFixtureMappings
			);
}
