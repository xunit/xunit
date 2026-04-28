namespace Xunit.v3;

/// <summary>
/// Test assembly runner for xUnit.net v3 tests (with overridable context).
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestAssemblyRunnerBase<TContext, TTestAssembly, TTestCollection, TTestCase> :
	CoreTestAssemblyRunner<TContext, TTestAssembly, TTestCollection, TTestCase>
		where TContext : XunitTestAssemblyRunnerBaseContext<TTestAssembly, TTestCollection, TTestCase>
		where TTestAssembly : class, IXunitTestAssembly
		where TTestCollection : class, IXunitTestCollection
		where TTestCase : class, IXunitTestCase
{
	/// <inheritdoc/>
	protected override ValueTask<string> GetTestFrameworkDisplayName(TContext ctxt) =>
		new(XunitTestFramework.DisplayName);

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestAssemblyFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.AssemblyFixtureMappings.DisposeAsync);
		return await base.OnTestAssemblyFinished(ctxt, summary);
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestAssemblyStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = await base.OnTestAssemblyStarting(ctxt);
		await ctxt.Aggregator.RunAsync(() => ctxt.AssemblyFixtureMappings.InitializeAsync(
			ctxt.TestAssembly.AssemblyFixtureTypes,
			createInstances: ctxt.TestCases.Any(tc => !tc.IsStaticallySkipped())
		));
		return result;
	}

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestCollections(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (exception is not null)
			return await base.RunTestCollections(ctxt, exception);

		using var lifecycleTracker = new NotificationTracker<INotifyTestAssemblyLifecycle>(
			ctxt.AssemblyFixtureMappings.ForNotification<INotifyTestAssemblyLifecycle>(),
			fixture => fixture.OnTestAssemblyStarting(ctxt.TestAssembly),
			fixture => ctxt.Aggregator.Run(() => fixture.OnTestAssemblyFinished(ctxt.TestAssembly)),
			ctxt.CancellationTokenSource.Token
		);
		await using var lifecycleAsyncTracker = new NotificationTrackerAsync<INotifyTestAssemblyLifecycleAsync>(
			ctxt.AssemblyFixtureMappings.ForNotification<INotifyTestAssemblyLifecycleAsync>(),
			fixture => fixture.OnTestAssemblyStartingAsync(ctxt.TestAssembly),
			fixture => ctxt.Aggregator.RunAsync(() => fixture.OnTestAssemblyFinishedAsync(ctxt.TestAssembly)),
			ctxt.CancellationTokenSource.Token
		);

		var aggregator = lifecycleTracker.Up();

		if (!aggregator.HasExceptions)
			aggregator.Aggregate(await lifecycleAsyncTracker.Up());

		return await base.RunTestCollections(ctxt, aggregator.ToException());
	}
}
