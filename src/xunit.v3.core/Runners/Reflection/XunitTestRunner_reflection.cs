using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Test runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestRunner : XunitTestRunnerBase<XunitTestRunnerContext, IXunitTest>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestRunner"/> class.
	/// </summary>
	protected XunitTestRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitTestRunner"/>.
	/// </summary>
	public static XunitTestRunner Instance = new();

	/// <summary>
	/// Please call <see cref="Run(IXunitTest, IMessageBus, object?[], ExplicitOption, ExceptionAggregator, CancellationTokenSource, ParallelMode, ExecutionScheduler, IReadOnlyCollection{IBeforeAfterTestAttribute}, FixtureMappingManager)"/>.
	/// This overload is no longer valid and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call the overload which adds parallelMode, scheduler, and caseFixtureMappings. This overload is no longer valid and will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public ValueTask<RunSummary> Run(
		IXunitTest test,
		IMessageBus messageBus,
		object?[] constructorArguments,
		ExplicitOption explicitOption,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterAttributes) =>
			throw new NotSupportedException("Please call the overload which adds parallelMode, scheduler, and caseFixtureMappings. This overload is no longer valid and will be removed in the next major version.");

	/// <summary>
	/// Runs the test.
	/// </summary>
	/// <param name="test">The test that this invocation belongs to.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="parallelMode">The parallel mode for the test</param>
	/// <param name="scheduler">The scheduler used for task/test scheduling</param>
	/// <param name="beforeAfterAttributes">The list of <see cref="IBeforeAfterTestAttribute"/>s for this test.</param>
	/// <param name="caseFixtureMappings">The fixtures attached to the test case</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	public async ValueTask<RunSummary> Run(
		IXunitTest test,
		IMessageBus messageBus,
		object?[] constructorArguments,
		ExplicitOption explicitOption,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		ParallelMode parallelMode,
		ExecutionScheduler scheduler,
		IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterAttributes,
		FixtureMappingManager caseFixtureMappings)
	{
		await using var ctxt = new XunitTestRunnerContext(
			test,
			explicitOption,
			messageBus,
			aggregator,
			cancellationTokenSource,
			parallelMode,
			scheduler,
			beforeAfterAttributes,
			constructorArguments,
			caseFixtureMappings
		);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}
}
