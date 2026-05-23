namespace Xunit.v3;

/// <summary>
/// The test assembly runner for xUnit.net v3 tests (with overridable context).
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public abstract class CodeGenTestAssemblyRunnerBase<TContext, TTestAssembly, TTestCollection, TTestCase> :
	CoreTestAssemblyRunner<TContext, TTestAssembly, TTestCollection, TTestCase>
		where TContext : CodeGenTestAssemblyRunnerBaseContext<TTestAssembly, TTestCollection, TTestCase>
		where TTestAssembly : class, ICodeGenTestAssembly
		where TTestCollection : class, ICodeGenTestCollection
		where TTestCase : class, ICodeGenTestCase
{
	/// <inheritdoc/>
	protected override ValueTask<string> GetTestFrameworkDisplayName(TContext ctxt) =>
		new(CodeGenTestFramework.DisplayName);

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
			fixture => fixture.OnTestAssemblyFinished(ctxt.TestAssembly),
			ctxt.CancellationTokenSource.Token
		);
		await using var lifecycleAsyncTracker = new NotificationTrackerAsync<INotifyTestAssemblyLifecycleAsync>(
			ctxt.AssemblyFixtureMappings.ForNotification<INotifyTestAssemblyLifecycleAsync>(),
			fixture => fixture.OnTestAssemblyStartingAsync(ctxt.TestAssembly),
			fixture => fixture.OnTestAssemblyFinishedAsync(ctxt.TestAssembly),
			ctxt.CancellationTokenSource.Token
		);

		var aggregator = lifecycleTracker.Up();

		if (!aggregator.HasExceptions)
			aggregator.Aggregate(await lifecycleAsyncTracker.Up());

		return await base.RunTestCollections(ctxt, aggregator.ToException());
	}
}
