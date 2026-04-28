using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test case runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestCaseRunner : CoreTestCaseRunner<CodeGenTestCaseRunnerContext, ICodeGenTestCase, ICodeGenTest>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CodeGenTestCaseRunner"/> class.
	/// </summary>
	protected CodeGenTestCaseRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="CodeGenTestCaseRunner"/> class.
	/// </summary>
	public static CodeGenTestCaseRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test case.
	/// </summary>
	/// <remarks>
	/// This entry point is used for both single-test (like <see cref="FactAttribute"/> and individual data
	/// rows for <see cref="TheoryAttribute"/> tests) and multi-test test cases (like <see cref="TheoryAttribute"/>
	/// when pre-enumeration is disable or the theory data was not serializable).
	/// </remarks>
	/// <param name="testCase">The test case that this invocation belongs to.</param>
	/// <param name="tests">The tests for the test case.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="displayName">The display name of the test case.</param>
	/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="methodFixtureMappings">The mapping of method fixture types to fixtures.</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	public async ValueTask<RunSummary> Run(
		ICodeGenTestCase testCase,
		IReadOnlyCollection<ICodeGenTest> tests,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		string displayName,
		string? skipReason,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager methodFixtureMappings)
	{
		await using var ctxt = new CodeGenTestCaseRunnerContext(
			testCase,
			tests,
			explicitOption,
			messageBus,
			aggregator,
			displayName,
			skipReason,
			cancellationTokenSource,
			methodFixtureMappings
		);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestCase(
		CodeGenTestCaseRunnerContext ctxt,
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
