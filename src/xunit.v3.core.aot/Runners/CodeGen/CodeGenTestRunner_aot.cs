using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Test runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestRunner : CoreTestRunner<CodeGenTestRunnerContext, ICodeGenTest, BeforeAfterTestAttribute>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CodeGenTestRunner"/> class.
	/// </summary>
	protected CodeGenTestRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="CodeGenTestRunner"/>.
	/// </summary>
	public static CodeGenTestRunner Instance = new();

	/// <inheritdoc/>
	protected override ValueTask<(object? Instance, SynchronizationContext? SyncContext, ExecutionContext? ExecutionContext)> CreateTestClassInstance(CodeGenTestRunnerContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).CreateTestClassInstance();

	/// <inheritdoc/>
	protected override bool IsTestClassCreatable(CodeGenTestRunnerContext ctxt) =>
		!Guard.ArgumentNotNull(ctxt).Test.TestCase.TestMethod.IsStatic;

	/// <summary>
	/// Runs the test.
	/// </summary>
	/// <param name="test">The test that this invocation belongs to.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="caseFixtureMappings">The mapping of test case fixture types to fixtures.</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	public async ValueTask<RunSummary> Run(
		ICodeGenTest test,
		IMessageBus messageBus,
		ExplicitOption explicitOption,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager caseFixtureMappings)
	{
		await using var ctxt = new CodeGenTestRunnerContext(
			Guard.ArgumentNotNull(test),
			messageBus,
			explicitOption,
			aggregator,
			cancellationTokenSource,
			caseFixtureMappings
		);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}

	/// <inheritdoc/>
	protected override async ValueTask<TimeSpan> RunTest(CodeGenTestRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		using var lifecycleTracker = new NotificationTracker<INotifyTestLifecycle>(
			ctxt.TestFixtureMappings.ForNotification<INotifyTestLifecycle>(),
			fixture => fixture.OnTestStarting(ctxt.Test),
			fixture => ctxt.Aggregator.Run(() => fixture.OnTestFinished(ctxt.Test)),
			ctxt.CancellationTokenSource.Token
		);
		await using var lifecycleAsyncTracker = new NotificationTrackerAsync<INotifyTestLifecycleAsync>(
			ctxt.TestFixtureMappings.ForNotification<INotifyTestLifecycleAsync>(),
			fixture => fixture.OnTestStartingAsync(ctxt.Test),
			fixture => ctxt.Aggregator.RunAsync(() => fixture.OnTestFinishedAsync(ctxt.Test)),
			ctxt.CancellationTokenSource.Token
		);

		ctxt.Aggregator.Aggregate(lifecycleTracker.Up());

		if (!ctxt.Aggregator.HasExceptions)
			ctxt.Aggregator.Aggregate(await lifecycleAsyncTracker.Up());

		if (!ctxt.Aggregator.HasExceptions)
			return await base.RunTest(ctxt);

		return TimeSpan.Zero;
	}
}
