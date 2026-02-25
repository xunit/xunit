using System.Diagnostics;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base test runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is shared between reflection-based and code generation-based tests.
/// </remarks>
public abstract class CoreTestRunner<TContext, TTest, TBeforeAfterAttribute> : TestRunner<TContext, TTest>
	where TContext : CoreTestRunnerContext<TTest, TBeforeAfterAttribute>
	where TTest : class, ICoreTest
	where TBeforeAfterAttribute : notnull
{
	/// <inheritdoc/>
	protected override ValueTask<IReadOnlyDictionary<string, TestAttachment>?> GetAttachments(TContext ctxt) =>
		new(TestContext.Current.Attachments);

	/// <inheritdoc/>
	protected override ValueTask<string> GetTestOutput(TContext ctxt) =>
		new(TestContext.Current.TestOutputHelper?.Output ?? string.Empty);

	/// <inheritdoc/>
	protected override ValueTask<string[]?> GetWarnings(TContext ctxt) =>
		new(TestContext.Current.Warnings?.ToArray());

	/// <inheritdoc/>
	protected override ValueTask<TimeSpan> InvokeTest(
		TContext ctxt,
		object? testClassInstance)
	{
		Guard.ArgumentNotNull(ctxt);

		return ctxt.InvokeTest(testClassInstance);
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return OnTestStarting(ctxt, ctxt.Test.Explicit, ctxt.Test.Timeout);
	}

	/// <inheritdoc/>
	protected override void PostInvoke(TContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).RunAfterAttributes();

	/// <inheritdoc/>
	protected override void PreInvoke(TContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).RunBeforeAttributes();

	/// <inheritdoc/>
	protected override ValueTask<TimeSpan> RunTest(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return
			ctxt.Test.Timeout > 0
				? RunTestWithTimeout(ctxt, ctxt.Test.Timeout)
				: base.RunTest(ctxt);
	}

	async ValueTask<TimeSpan> RunTestWithTimeout(
		TContext ctxt,
		int timeout)
	{
		// We keep our own notion of execution time, since the time recorded by
		// calling the base RunTest isn't sufficient
		var stopwatch = Stopwatch.StartNew();

		await ctxt.Aggregator.RunAsync(async () =>
		{
			var syncContext = SynchronizationContext.Current;

			Task baseTask =
				syncContext is null
					? Task.Run(async () => await base.RunTest(ctxt))
					: Task.Run(() =>
					{
						var tcs = new TaskCompletionSource<object?>();

						syncContext.Post(async _ =>
						{
							// base.RunTest is guarded against throwing, so no need to
							// try/catch and report exceptions via the TCS
							await base.RunTest(ctxt);
							tcs.TrySetResult(null);
						}, null);

						return tcs.Task;
					});

			var resultTask = await Task.WhenAny(baseTask, Task.Delay(timeout));

			if (resultTask != baseTask)
			{
				try
				{
					var timeoutException = TestTimeoutException.ForTimedOutTest(timeout);
					UpdateTestContext(null, TestResultState.FromException(timeout * 1000, timeoutException));
					throw timeoutException;
				}
				finally
				{
					TestContext.Current.CancelCurrentTest();
				}
			}
		});

		return stopwatch.Elapsed;
	}

	/// <inheritdoc/>
	protected override bool ShouldTestRun(TContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).ExplicitOption switch
		{
			ExplicitOption.Only => ctxt.Test.Explicit,
			ExplicitOption.Off => !ctxt.Test.Explicit,
			_ => true,
		};
}
