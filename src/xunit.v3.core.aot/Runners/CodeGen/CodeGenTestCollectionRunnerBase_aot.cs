namespace Xunit.v3;

/// <summary>
/// The test collection runner for xUnit.net v3 tests (with overridable context).
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public abstract class CodeGenTestCollectionRunnerBase<TContext, TTestCollection, TTestClass, TTestCase> :
	CoreTestCollectionRunner<TContext, ICodeGenTestCollection, ICodeGenTestClass, ICodeGenTestCase>
		where TContext : CodeGenTestCollectionRunnerContext
		where TTestCollection : class, ICodeGenTestCollection
		where TTestClass : class, ICodeGenTestClass
		where TTestCase : class, ICodeGenTestCase
{
	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestCollectionFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.CollectionFixtureMappings.DisposeAsync);
		return await base.OnTestCollectionFinished(ctxt, summary);
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestCollectionStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = await base.OnTestCollectionStarting(ctxt);
		await ctxt.Aggregator.RunAsync(() => ctxt.CollectionFixtureMappings.InitializeAsync(
			createInstances: ctxt.TestCases.Any(tc => !tc.IsStaticallySkipped())
		));
		return result;
	}

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestClasses(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (exception is not null)
			return await base.RunTestClasses(ctxt, exception);

		using var lifecycleTracker = new NotificationTracker<INotifyTestCollectionLifecycle>(
			ctxt.CollectionFixtureMappings.ForNotification<INotifyTestCollectionLifecycle>(),
			fixture => fixture.OnTestCollectionStarting(ctxt.TestCollection),
			fixture => ctxt.Aggregator.Run(() => fixture.OnTestCollectionFinished(ctxt.TestCollection)),
			ctxt.CancellationTokenSource.Token
		);
		await using var lifecycleAsyncTracker = new NotificationTrackerAsync<INotifyTestCollectionLifecycleAsync>(
			ctxt.CollectionFixtureMappings.ForNotification<INotifyTestCollectionLifecycleAsync>(),
			fixture => fixture.OnTestCollectionStartingAsync(ctxt.TestCollection),
			fixture => ctxt.Aggregator.RunAsync(() => fixture.OnTestCollectionFinishedAsync(ctxt.TestCollection)),
			ctxt.CancellationTokenSource.Token
		);

		var aggregator = lifecycleTracker.Up();

		if (!aggregator.HasExceptions)
			aggregator.Aggregate(await lifecycleAsyncTracker.Up());

		return await base.RunTestClasses(ctxt, aggregator.ToException());
	}
}
