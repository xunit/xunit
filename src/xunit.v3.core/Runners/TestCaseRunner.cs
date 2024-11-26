using System;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running test cases which are assumed
/// to result in one or more tests (that implement <see cref="ITest"/>).
/// </summary>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
/// <typeparam name="TTest">The type of the test that is generated from the test case. Must
/// derive from <see cref="ITest"/>.</typeparam>
public abstract class TestCaseRunner<TContext, TTestCase, TTest> :
	TestCaseRunnerBase<TContext, TTestCase>
		where TContext : TestCaseRunnerContext<TTestCase, TTest>
		where TTestCase : class, ITestCase
		where TTest : class, ITest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestCaseRunner{TContext, TTestCase, TTest}"/> class.
	/// </summary>
	protected TestCaseRunner()
	{ }

	/// <summary>
	/// Override this method to fail an individual test.
	/// </summary>
	/// <remarks>
	/// By default, uses <see cref="XunitRunnerHelper"/> to fail the test cases.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test case</param>
	/// <param name="test">The test to be failed.</param>
	/// <param name="exception">The exception that was caused during startup.</param>
	/// <returns>Returns summary information about the test case run.</returns>
	protected virtual ValueTask<RunSummary> FailTest(
		TContext ctxt,
		TTest test,
		Exception exception) =>
			new(XunitRunnerHelper.FailTest(
				Guard.ArgumentNotNull(ctxt).MessageBus,
				ctxt.CancellationTokenSource,
				test,
				exception
			));

	/// <summary>
	/// Override this method to run an individual test.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <param name="test">The test to be run.</param>
	/// <returns>Returns summary information about the test case run.</returns>
	protected abstract ValueTask<RunSummary> RunTest(
		TContext ctxt,
		TTest test);

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestCase(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var summary = new RunSummary();

		foreach (var test in ctxt.Tests)
		{
			if (exception is not null)
				summary.Aggregate(await FailTest(ctxt, test, exception));
			else
				summary.Aggregate(await RunTest(ctxt, test));

			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				break;
		}

		return summary;
	}
}
