using System;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running tests in a test method.
/// </summary>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTestMethod">The type of the test method used by the test framework.
/// Must derive from <see cref="ITestMethod"/>.</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
public abstract class TestMethodRunner<TContext, TTestMethod, TTestCase>
	where TContext : TestMethodRunnerContext<TTestMethod, TTestCase>
	where TTestMethod : class, ITestMethod
	where TTestCase : class, ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestMethodRunner{TContext, TTestCase, TTestMethod}"/> class.
	/// </summary>
	protected TestMethodRunner()
	{ }

	/// <summary>
	/// Override this method to fail an individual test case.
	/// </summary>
	/// <remarks>
	/// By default, uses <see cref="XunitRunnerHelper"/> to fail the test cases.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <param name="testCase">The test case to be failed.</param>
	/// <param name="exception">The exception that was caused during startup.</param>
	/// <returns>Returns summary information about the test case run.</returns>
	protected virtual ValueTask<RunSummary> FailTestCase(
		TContext ctxt,
		TTestCase testCase,
		Exception exception) =>
			new(XunitRunnerHelper.FailTestCases(
				Guard.ArgumentNotNull(ctxt).MessageBus,
				ctxt.CancellationTokenSource,
				[testCase],
				exception
			));

	/// <summary>
	/// This method is called when an exception was thrown by <see cref="OnTestMethodFinished"/>. By default, this
	/// sends <see cref="ErrorMessage"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/>. It must never throw an exception.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="exception">The exception that was thrown by <see cref="OnTestMethodFinished"/>.</param>
	protected virtual ValueTask<bool> OnError(
		TContext ctxt,
		Exception exception) =>
			new(Guard.ArgumentNotNull(ctxt).MessageBus.QueueMessage(ErrorMessage.FromException(exception)));

	/// <summary>
	/// This method is called when an exception was thrown while cleaning up, after the test method
	/// has run. By default, this sends <see cref="TestMethodCleanupFailure"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown are
	/// converted into fatal exception messages (via <see cref="IErrorMessage"/>) and sent to the message
	/// bus in <paramref name="ctxt"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <param name="exception">The exception that caused the cleanup failure (may be an instance
	/// of <see cref="AggregateException"/> if more than one exception occurred).</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestMethodCleanupFailure(
		TContext ctxt,
		Exception exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var (types, messages, stackTraces, indices, _) = ExceptionUtility.ExtractMetadata(exception);

		return new(ctxt.MessageBus.QueueMessage(new TestMethodCleanupFailure
		{
			AssemblyUniqueID = ctxt.TestMethod.TestClass.TestCollection.TestAssembly.UniqueID,
			ExceptionParentIndices = indices,
			ExceptionTypes = types,
			Messages = messages,
			StackTraces = stackTraces,
			TestClassUniqueID = ctxt.TestMethod.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestMethod.TestClass.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.TestMethod.UniqueID,
		}));
	}

	/// <summary>
	/// This method will be called when the test method has finished running. By default, this sends
	/// <see cref="TestMethodFinished"/>. Override this to enable any extensibility related to test
	/// method finish.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// be reported as top-level exceptions. Any exceptions that are present in the aggregator (presumably
	/// from derived implementations of this method, <see cref="RunTestCases"/>, or <see cref="RunTestCase"/>)
	/// will invoke <see cref="OnTestMethodCleanupFailure"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <param name="summary">The execution summary for the test method</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual async ValueTask<bool> OnTestMethodFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		if (ctxt.Aggregator.HasExceptions)
		{
			var exception = ctxt.Aggregator.ToException()!;
			ctxt.Aggregator.Clear();

			if (!await ctxt.Aggregator.RunAsync(() => OnTestMethodCleanupFailure(ctxt, exception), true))
				ctxt.CancellationTokenSource.Cancel();
		}

		return ctxt.MessageBus.QueueMessage(new TestMethodFinished
		{
			AssemblyUniqueID = ctxt.TestMethod.TestClass.TestCollection.TestAssembly.UniqueID,
			ExecutionTime = summary.Time,
			TestClassUniqueID = ctxt.TestMethod.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestMethod.TestClass.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.TestMethod.UniqueID,
			TestsFailed = summary.Failed,
			TestsNotRun = summary.NotRun,
			TestsSkipped = summary.Skipped,
			TestsTotal = summary.Total,
		});
	}

	/// <summary>
	/// This method will be called before the test method has started running. By default, this sends
	/// <see cref="TestMethodStarting"/>. Override this to enable any extensibility related to test
	/// method start.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Initializing"/> and any exceptions thrown will
	/// contribute to test method failure (and will prevent the test method from running). Even if
	/// this method records exceptions, <see cref="OnTestMethodFinished"/> will be called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestMethodStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestMethodStarting
		{
			AssemblyUniqueID = ctxt.TestMethod.TestClass.TestCollection.TestAssembly.UniqueID,
			MethodName = Guard.ArgumentNotNull(ctxt).TestMethod.MethodName,
			TestClassUniqueID = ctxt.TestMethod.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestMethod.TestClass.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.TestMethod.UniqueID,
			Traits = ctxt.TestMethod.Traits,
		}));
	}

	/// <summary>
	/// Runs the tests in the test method.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected async ValueTask<RunSummary> Run(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		var summary = default(RunSummary);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestMethodStarting(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		SetTestContext(ctxt, TestEngineStatus.Running);

		var startingException = ctxt.Aggregator.ToException();
		ctxt.Aggregator.Clear();

		if (!ctxt.CancellationTokenSource.IsCancellationRequested)
			summary = await ctxt.Aggregator.RunAsync(() => RunTestCases(ctxt, startingException), default);

		SetTestContext(ctxt, TestEngineStatus.CleaningUp);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestMethodFinished(ctxt, summary), true))
			ctxt.CancellationTokenSource.Cancel();

		if (ctxt.Aggregator.HasExceptions)
			if (!await ctxt.Aggregator.RunAsync(() => OnError(ctxt, ctxt.Aggregator.ToException()!), true))
				ctxt.CancellationTokenSource.Cancel();

		ctxt.Aggregator.Clear();

		return summary;
	}

	/// <summary>
	/// Runs the list of test cases. By default, it runs the cases in order, synchronously.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test method cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected virtual async ValueTask<RunSummary> RunTestCases(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var summary = new RunSummary();

		foreach (var testCase in ctxt.TestCases)
		{
			if (exception is not null)
				summary.Aggregate(await FailTestCase(ctxt, testCase, exception));
			else
				summary.Aggregate(await RunTestCase(ctxt, testCase));

			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				break;
		}

		return summary;
	}

	/// <summary>
	/// Override this method to run an individual test case.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test method cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <param name="testCase">The test case to be run.</param>
	/// <returns>Returns summary information about the test case run.</returns>
	protected abstract ValueTask<RunSummary> RunTestCase(
		TContext ctxt,
		TTestCase testCase);

	/// <summary>
	/// Sets the current <see cref="TestContext"/> for the current test method and the given test method status.
	/// </summary>
	/// <remarks>
	/// This method must never throw. Behavior is undefined if it does. Instead, exceptions that
	/// occur should be recorded in the aggregator in <paramref name="ctxt"/> and will be reflected
	/// in a way that's appropriate based on when this method is called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test method</param>
	/// <param name="testMethodStatus">The current test method status.</param>
	protected virtual void SetTestContext(
		TContext ctxt,
		TestEngineStatus testMethodStatus)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTestMethod(ctxt.TestMethod, testMethodStatus, ctxt.CancellationTokenSource.Token);
	}
}
