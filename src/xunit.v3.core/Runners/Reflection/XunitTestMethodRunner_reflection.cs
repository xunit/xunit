using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Test method runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestMethodRunner : XunitTestMethodRunnerBase<XunitTestMethodRunnerContext, IXunitTestMethod, IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestMethodRunner"/> class.
	/// </summary>
	protected XunitTestMethodRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitTestMethodRunner"/> class.
	/// </summary>
	public static XunitTestMethodRunner Instance { get; } = new();

	/// <summary>
	/// Please call <see cref="Run(IXunitTestMethod, IReadOnlyCollection{IXunitTestCase}, ExplicitOption, IMessageBus, ExceptionAggregator, CancellationTokenSource, ParallelMode, ExecutionScheduler, object?[], FixtureMappingManager)"/>.
	/// This overload is no longer valid and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call the overload which adds parallelMode and scheduler. This overload is no longer valid and will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ValueTask<RunSummary> Run(
		IXunitTestMethod testMethod,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		object?[] constructorArguments) =>
			throw new NotSupportedException("Please call the overload which adds parallelMode and scheduler. This overload is no longer valid and will be removed in the next major version.");

	/// <summary>
	/// Runs the test test method.
	/// </summary>
	/// <param name="testMethod">The test method to be run.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="parallelMode">The parallel mode for the test method</param>
	/// <param name="scheduler">The scheduler used for task/test scheduling</param>
	/// <param name="constructorArguments">The constructor arguments for the test class.</param>
	/// <param name="classFixtureMappings">The fixtures attached to the test class</param>
	public async ValueTask<RunSummary> Run(
		IXunitTestMethod testMethod,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		ParallelMode parallelMode,
		ExecutionScheduler scheduler,
		object?[] constructorArguments,
		FixtureMappingManager classFixtureMappings)
	{
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(constructorArguments);

		await using var ctxt = new XunitTestMethodRunnerContext(
			testMethod,
			testCases,
			explicitOption,
			messageBus,
			aggregator,
			cancellationTokenSource,
			parallelMode,
			scheduler,
			constructorArguments,
			classFixtureMappings
		);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}
}
