namespace Xunit.v3;

/// <summary>
/// Test case runner for xUnit.net v3 tests (with overridable context).
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public abstract class XunitTestCaseRunnerBase<TContext, TTestCase, TTest> :
	CoreTestCaseRunner<TContext, TTestCase, TTest>
		where TContext : XunitTestCaseRunnerBaseContext<TTestCase, TTest>
		where TTestCase : class, IXunitTestCase
		where TTest : class, IXunitTest
{
	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestCase(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (exception is not null)
			return await base.RunTestCase(ctxt, exception);

		using var lifecycleTracker = new NotificationTracker<INotifyTestCaseLifecycle>(
			ctxt.CaseFixtureMappings.ForNotification<INotifyTestCaseLifecycle>(),
			fixture => fixture.OnTestCaseStarting(ctxt.TestCase),
			fixture => ctxt.Aggregator.Run(() => fixture.OnTestCaseFinished(ctxt.TestCase)),
			ctxt.CancellationTokenSource.Token
		);
		await using var lifecycleAsyncTracker = new NotificationTrackerAsync<INotifyTestCaseLifecycleAsync>(
			ctxt.CaseFixtureMappings.ForNotification<INotifyTestCaseLifecycleAsync>(),
			fixture => fixture.OnTestCaseStartingAsync(ctxt.TestCase),
			fixture => ctxt.Aggregator.RunAsync(() => fixture.OnTestCaseFinishedAsync(ctxt.TestCase)),
			ctxt.CancellationTokenSource.Token
		);

		var aggregator = lifecycleTracker.Up();

		if (!aggregator.HasExceptions)
			aggregator.Aggregate(await lifecycleAsyncTracker.Up());

		return await base.RunTestCase(ctxt, aggregator.ToException());
	}
}
