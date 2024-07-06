using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="TestCaseRunner{TContext, TTestCase}"/> to support <see cref="ExecutionErrorTestCase"/>.
/// </summary>
public class ExecutionErrorTestCaseRunner :
	XunitTestCaseRunnerBase<XunitTestCaseRunnerContext<ExecutionErrorTestCase>, ExecutionErrorTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ExecutionErrorTestCaseRunner"/> class.
	/// </summary>
	protected ExecutionErrorTestCaseRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="ExecutionErrorTestCaseRunner"/> class.
	/// </summary>
	public static ExecutionErrorTestCaseRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test case.
	/// </summary>
	/// <param name="testCase">The test case that this invocation belongs to.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	public async ValueTask<RunSummary> RunAsync(
		ExecutionErrorTestCase testCase,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		Guard.ArgumentNotNull(testCase);

		await using var ctxt = new XunitTestCaseRunnerContext<ExecutionErrorTestCase>(testCase, messageBus, aggregator, cancellationTokenSource, testCase.TestCaseDisplayName, string.Empty, ExplicitOption.Off, [], []);
		await ctxt.InitializeAsync();

		return await RunAsync(ctxt);
	}

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestsAsync(
		XunitTestCaseRunnerContext<ExecutionErrorTestCase> ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		Exception executionException = new TestPipelineException(ctxt.TestCase.ErrorMessage);
		if (exception is not null)
			executionException = new AggregateException(executionException, exception);

		return new(XunitRunnerHelper.FailTestCases(ctxt.MessageBus, ctxt.CancellationTokenSource, [ctxt.TestCase], executionException, sendTestCaseMessages: false));
	}
}
