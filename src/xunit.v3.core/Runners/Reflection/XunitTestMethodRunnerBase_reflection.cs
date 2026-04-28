namespace Xunit.v3;

/// <summary>
/// Test method runner for xUnit.net v3 tests (with overridable context).
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestMethodRunnerBase<TContext, TTestMethod, TTestCase> :
	CoreTestMethodRunner<TContext, TTestMethod, TTestCase>
		where TContext : XunitTestMethodRunnerBaseContext<TTestMethod, TTestCase>
		where TTestMethod : class, IXunitTestMethod
		where TTestCase : class, IXunitTestCase
{
	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestCases(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (exception is not null)
			return await base.RunTestCases(ctxt, exception);

		await using var lifecycleTracker = new NotificationTracker<INotifyTestMethodLifecycle>(
			ctxt.MethodFixtureMappings.ForNotification<INotifyTestMethodLifecycle>(),
			fixture => fixture.OnTestMethodStarting(ctxt.TestMethod),
			fixture => ctxt.Aggregator.Run(() => fixture.OnTestMethodFinished(ctxt.TestMethod)),
			ctxt.CancellationTokenSource.Token
		);
		await using var lifecycleAsyncTracker = new NotificationTracker<INotifyTestMethodLifecycleAsync>(
			ctxt.MethodFixtureMappings.ForNotification<INotifyTestMethodLifecycleAsync>(),
			fixture => fixture.OnTestMethodStartingAsync(ctxt.TestMethod),
			fixture => ctxt.Aggregator.RunAsync(() => fixture.OnTestMethodFinishedAsync(ctxt.TestMethod)),
			ctxt.CancellationTokenSource.Token
		);

		var aggregator = await lifecycleTracker.Up();

		if (!aggregator.HasExceptions)
			aggregator.Aggregate(await lifecycleAsyncTracker.Up());

		return await base.RunTestCases(ctxt, aggregator.ToException());
	}
}
