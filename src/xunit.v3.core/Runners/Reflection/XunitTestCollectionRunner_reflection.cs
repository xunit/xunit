using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Test collection runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestCollectionRunner :
	XunitTestCollectionRunnerBase<XunitTestCollectionRunnerContext, IXunitTestCollection, IXunitTestClass, IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestCollectionRunner"/> class.
	/// </summary>
	protected XunitTestCollectionRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of <see cref="XunitTestCollectionRunner"/>.
	/// </summary>
	public static XunitTestCollectionRunner Instance { get; } = new();

	/// <summary>
	/// Please call <see cref="Run(IXunitTestCollection, IReadOnlyCollection{IXunitTestCase}, ExplicitOption, IMessageBus, ExceptionAggregator, CancellationTokenSource, ParallelMode, ExecutionScheduler, FixtureMappingManager)"/>.
	/// This overload is no longer valid and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please use the overload which removes testCaseOrderer and adds parallelMode and scheduler. This overload is no longer valid and will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[OverloadResolutionPriority(-1)]
	public ValueTask<RunSummary> Run(
		IXunitTestCollection testCollection,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager assemblyFixtureMappings) =>
			throw new NotSupportedException("Please use the overload which removes testCaseOrderer and adds parallelMode and scheduler. This overload is no longer valid and will be removed in the next major version.");

	/// <summary>
	/// Runs the test collection.
	/// </summary>
	/// <param name="testCollection">The test collection to be run.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="parallelMode">The parallel mode for the test collection</param>
	/// <param name="scheduler">The scheduler used for task/test scheduling</param>
	/// <param name="assemblyFixtureMappings">The mapping manager for assembly fixtures.</param>
	public async ValueTask<RunSummary> Run(
		IXunitTestCollection testCollection,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		ParallelMode parallelMode,
		ExecutionScheduler scheduler,
		FixtureMappingManager assemblyFixtureMappings)
	{
		Guard.ArgumentNotNull(testCollection);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(cancellationTokenSource);
		Guard.ArgumentNotNull(scheduler);
		Guard.ArgumentNotNull(assemblyFixtureMappings);

		await using var ctxt = new XunitTestCollectionRunnerContext(
			testCollection,
			testCases,
			explicitOption,
			messageBus,
			aggregator,
			cancellationTokenSource,
			parallelMode,
			scheduler,
			assemblyFixtureMappings
		);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}
}
