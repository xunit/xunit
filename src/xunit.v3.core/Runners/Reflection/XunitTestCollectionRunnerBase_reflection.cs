using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Test collection runner for xUnit.net v3 tests (with overridable context).
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public abstract class XunitTestCollectionRunnerBase<TContext, TTestCollection, TTestClass, TTestCase> :
	CoreTestCollectionRunner<TContext, TTestCollection, TTestClass, TTestCase>
		where TContext : XunitTestCollectionRunnerBaseContext<TTestCollection, TTestClass, TTestCase>
		where TTestCollection : class, IXunitTestCollection
		where TTestClass : class, IXunitTestClass
		where TTestCase : class, IXunitTestCase
{
	/// <summary>
	/// This method is no longer called, and will be removed in the next major version.
	/// </summary>
	[Obsolete("This method is no longer called, and will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	protected ITestCaseOrderer? GetTestCaseOrderer(TContext ctxt) =>
		throw new NotSupportedException("This method is no longer called, and will be removed in the next major version.");

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
			ctxt.TestCollection.CollectionFixtureTypes,
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
