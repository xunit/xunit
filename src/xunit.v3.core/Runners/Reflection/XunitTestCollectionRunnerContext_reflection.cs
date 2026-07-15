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
/// <param name="parallelMode">The parallel mode for the test collection</param>
/// <param name="scheduler">The scheduler used for task/test scheduling</param>
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
	ParallelMode parallelMode,
	ExecutionScheduler scheduler,
	FixtureMappingManager assemblyFixtureMappings) :
		XunitTestCollectionRunnerBaseContext<IXunitTestCollection, IXunitTestClass, IXunitTestCase>(testCollection, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler, assemblyFixtureMappings)
{
	/// <summary>
	/// Please use <see cref="XunitTestCollectionRunnerContext(IXunitTestCollection, IReadOnlyCollection{IXunitTestCase}, ExplicitOption, IMessageBus, ExceptionAggregator, CancellationTokenSource, ParallelMode, ExecutionScheduler, FixtureMappingManager)"/>.
	/// This overload is no longer valid and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please use the constructor which removes testCaseOrderer and adds parallelMode and scheduler. This overload is no longer valid and will be removed in the next major version.", error: true)]
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
			this(testCollection, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, ParallelMode.None, ExecutionScheduler.Invalid, assemblyFixtureMappings) =>
				throw new NotSupportedException("Please use the constructor which removes testCaseOrderer and adds parallelMode and scheduler. This overload is no longer valid and will be removed in the next major version.");

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
				ParallelMode,
				Scheduler,
				CollectionFixtureMappings
			);
}
