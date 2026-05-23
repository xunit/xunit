namespace Xunit.v3;

/// <summary>
/// Test runner for xUnit.net v3 tests (with overridable context).
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestRunnerBase<TContext, TTest> : CoreTestRunner<TContext, TTest, BeforeAfterTestAttribute>
	where TContext : CodeGenTestRunnerBaseContext<TTest>
	where TTest : class, ICodeGenTest
{
	/// <inheritdoc/>
	protected override ValueTask<(object? Instance, SynchronizationContext? SyncContext, ExecutionContext? ExecutionContext)> CreateTestClassInstance(TContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).CreateTestClassInstance();

	/// <inheritdoc/>
	protected override bool IsTestClassCreatable(TContext ctxt) =>
		!Guard.ArgumentNotNull(ctxt).Test.TestCase.TestMethod.IsStatic;

	/// <inheritdoc/>
	protected override async ValueTask<TimeSpan> RunTest(TContext ctxt)
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
