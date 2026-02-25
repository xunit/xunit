namespace Xunit.v3;

/// <summary>
/// Base test assembly runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is shared between reflection-based and code generation-based tests.
/// </remarks>
public class CoreTestCaseRunner<TContext, TTestCase, TTest> : TestCaseRunner<TContext, TTestCase, TTest>
	where TContext : CoreTestCaseRunnerContext<TTestCase, TTest>
	where TTestCase : class, ICoreTestCase
	where TTest : class, ICoreTest
{
	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestCase(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var preInvokeFailed = true;

		if (exception is null)
		{
			try
			{
				ctxt.TestCase.PreInvoke();
				preInvokeFailed = false;
			}
			catch (Exception ex)
			{
				exception = ex;
			}
		}

		var result = await base.RunTestCase(ctxt, exception);

		if (!preInvokeFailed)
			ctxt.Aggregator.Run(ctxt.TestCase.PostInvoke);

		return result;
	}

	/// <summary>
	/// Runs the test via the context.
	/// </summary>
	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTest(
		TContext ctxt,
		TTest test) =>
			Guard.ArgumentNotNull(ctxt).RunTest(test);
}
