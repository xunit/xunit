namespace Xunit.v3;

/// <summary>
/// The test class runner for xUnit.net v3 tests (with overridable context).
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public abstract class CodeGenTestClassRunnerBase<TContext, TTestClass, TTestMethod, TTestCase> :
	CoreTestClassRunner<TContext, TTestClass, TTestMethod, TTestCase>
		where TContext : CodeGenTestClassRunnerBaseContext<TTestClass, TTestMethod, TTestCase>
		where TTestClass : class, ICodeGenTestClass
		where TTestMethod : class, ICodeGenTestMethod
		where TTestCase : class, ICodeGenTestCase
{
	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestClassFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.ClassFixtureMappings.DisposeAsync);
		return await base.OnTestClassFinished(ctxt, summary);
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestClassStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = await base.OnTestClassStarting(ctxt);
		await ctxt.Aggregator.RunAsync(() => ctxt.ClassFixtureMappings.InitializeAsync(
			createInstances: ctxt.TestCases.Any(tc => !tc.IsStaticallySkipped())
		));
		return result;
	}

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestMethods(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (exception is not null)
			return await base.RunTestMethods(ctxt, exception);

		using var lifecycleTracker = new NotificationTracker<INotifyTestClassLifecycle>(
			ctxt.ClassFixtureMappings.ForNotification<INotifyTestClassLifecycle>(),
			fixture => fixture.OnTestClassStarting(ctxt.TestClass),
			fixture => ctxt.Aggregator.Run(() => fixture.OnTestClassFinished(ctxt.TestClass)),
			ctxt.CancellationTokenSource.Token
		);
		await using var lifecycleAsyncTracker = new NotificationTrackerAsync<INotifyTestClassLifecycleAsync>(
			ctxt.ClassFixtureMappings.ForNotification<INotifyTestClassLifecycleAsync>(),
			fixture => fixture.OnTestClassStartingAsync(ctxt.TestClass),
			fixture => ctxt.Aggregator.RunAsync(() => fixture.OnTestClassFinishedAsync(ctxt.TestClass)),
			ctxt.CancellationTokenSource.Token
		);

		var aggregator = lifecycleTracker.Up();

		if (!aggregator.HasExceptions)
			aggregator.Aggregate(await lifecycleAsyncTracker.Up());

		return await base.RunTestMethods(ctxt, aggregator.ToException());
	}

	/// <inheritdoc/>
	protected override void SetTestContext(
		TContext ctxt,
		TestEngineStatus testClassStatus)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTestClass(ctxt.TestClass, testClassStatus, ctxt.CancellationTokenSource.Token, ctxt.ClassFixtureMappings);
	}
}
