using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Test class runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestClassRunner :
	XunitTestClassRunnerBase<XunitTestClassRunnerContext, IXunitTestClass, IXunitTestMethod, IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestClassRunner"/> class.
	/// </summary>
	protected XunitTestClassRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitTestClassRunner"/> class.
	/// </summary>
	public static XunitTestClassRunner Instance { get; } = new();

	/// <summary>
	/// Please call <see cref="Run(IXunitTestClass, IReadOnlyCollection{IXunitTestCase}, ExplicitOption, IMessageBus, ExceptionAggregator, CancellationTokenSource, ParallelMode, ExecutionScheduler, FixtureMappingManager)"/>.
	/// This overload is no longer valid and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call the overload that removes testCaseOrderer and adds parallelMode and scheduler. This overload is no longer valid and will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public async ValueTask<RunSummary> Run(
		IXunitTestClass testClass,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager collectionFixtureMappings) =>
			throw new NotSupportedException("Please call the overload that removes testCaseOrderer and adds parallelMode and scheduler. This overload is no longer valid and will be removed in the next major version.");

	/// <summary>
	/// Runs the test class.
	/// </summary>
	/// <param name="testClass">The test class to be run.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="parallelMode">The parallel mode for the test class</param>
	/// <param name="scheduler">The scheduler used for task/test scheduling</param>
	/// <param name="collectionFixtureMappings">The mapping of collection fixture types to fixtures.</param>
	public async ValueTask<RunSummary> Run(
		IXunitTestClass testClass,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		ParallelMode parallelMode,
		ExecutionScheduler scheduler,
		FixtureMappingManager collectionFixtureMappings)
	{
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(cancellationTokenSource);
		Guard.ArgumentNotNull(scheduler);
		Guard.ArgumentNotNull(collectionFixtureMappings);

		await using var ctxt = new XunitTestClassRunnerContext(
			testClass,
			@testCases,
			explicitOption,
			messageBus,
			aggregator,
			cancellationTokenSource,
			parallelMode,
			scheduler,
			collectionFixtureMappings
		);
		await ctxt.InitializeAsync();

		return await ctxt.Aggregator.RunAsync(() => Run(ctxt), default);
	}
}
