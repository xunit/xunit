using System;
using System.Threading.Tasks;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// The test case runner for xUnit.net v3 tests (with overridable context).
/// </summary>
public abstract class XunitTestCaseRunnerBase<TContext, TTestCase, TTest> :
	TestCaseRunner<TContext, TTestCase, TTest>
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
}
