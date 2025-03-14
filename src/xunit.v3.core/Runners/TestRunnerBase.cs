using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running a test. This includes support
/// for skipping tests.
/// </summary>
/// <remarks>
/// This class does not make any assumptions about what it means to run an individual test,
/// just that at some point, the test will be run. The intention with this base class is that
/// it can serve as a base for non-traditional tests (e.g., tests that are not derived from
/// invoking CLR methods).
/// </remarks>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTest">The test type used by the test framework. Must derive from
/// <see cref="ITest"/>.</typeparam>
public abstract class TestRunnerBase<TContext, TTest>
	where TContext : TestRunnerBaseContext<TTest>
	where TTest : class, ITest
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestRunner{TContext, TTest}"/> class.
	/// </summary>
	protected TestRunnerBase()
	{ }

	/// <summary>
	/// Gets the attachments for the test. If the test framework did not collect attachments
	/// (or does not support attachments), then it should return <c>null</c>.
	/// </summary>
	/// <remarks>
	/// By default, this method returns <see cref="ITestContext.Attachments"/> from the current context.
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	protected virtual ValueTask<IReadOnlyDictionary<string, TestAttachment>?> GetAttachments(TContext ctxt) =>
		new(default(IReadOnlyDictionary<string, TestAttachment>));

	/// <summary>
	/// Gets any output collected from the test after execution is complete. If the test framework
	/// did not collect any output, or does not support collecting output, then it should
	/// return <see cref="string.Empty"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	protected virtual ValueTask<string> GetTestOutput(TContext ctxt) =>
		new(string.Empty);

	/// <summary>
	/// Gets the warnings that will be reported during test results. By default, returns <c>null</c>,
	/// indicating that there were no warnings
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	protected virtual ValueTask<string[]?> GetWarnings(TContext ctxt) =>
		new(default(string[]));

	/// <summary>
	/// This method is called when an exception was thrown by <see cref="OnTestFinished"/>. By default, this
	/// sends <see cref="ErrorMessage"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/>. It must never throw an exception.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="exception">The exception that was thrown by <see cref="OnTestFinished"/>.</param>
	protected virtual ValueTask<bool> OnError(
		TContext ctxt,
		Exception exception) =>
			new(Guard.ArgumentNotNull(ctxt).MessageBus.QueueMessage(ErrorMessage.FromException(exception)));

	/// <summary>
	/// This method is called when an exception was thrown while cleaning up, after the test has run.
	/// By default, this sends <see cref="TestCleanupFailure"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown are
	/// converted into fatal exception messages (via <see cref="IErrorMessage"/>) and sent to the message
	/// bus in <paramref name="ctxt"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="exception">The exception that caused the cleanup failure (may be an instance
	/// of <see cref="AggregateException"/> if more than one exception occurred).</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestCleanupFailure(
		TContext ctxt,
		Exception exception)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(exception);

		var (types, messages, stackTraces, indices, _) = ExceptionUtility.ExtractMetadata(exception);

		return new(ctxt.MessageBus.QueueMessage(new TestCleanupFailure
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			ExceptionParentIndices = indices,
			ExceptionTypes = types,
			Messages = messages,
			StackTraces = stackTraces,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass?.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
		}));
	}

	/// <summary>
	/// This method is called when a test has failed. By default, this sends <see cref="TestFailed"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="exception">The exception that caused the test failure</param>
	/// <param name="executionTime">The time spent running the test</param>
	/// <param name="output">The output from the test</param>
	/// <param name="warnings">The warnings that were generated during the test</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<(bool Continue, TestResultState ResultState)> OnTestFailed(
		TContext ctxt,
		Exception exception,
		decimal executionTime,
		string output,
		string[]? warnings)
	{
		Guard.ArgumentNotNull(ctxt);

		var (types, messages, stackTraces, indices, cause) = ExceptionUtility.ExtractMetadata(exception);

		var message = new TestFailed
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			Cause = cause,
			ExceptionParentIndices = indices,
			ExceptionTypes = types,
			ExecutionTime = executionTime,
			FinishTime = DateTimeOffset.UtcNow,
			Messages = messages,
			Output = output,
			StackTraces = stackTraces,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass?.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
			Warnings = warnings,
		};

		return new((ctxt.MessageBus.QueueMessage(message), TestResultState.FromTestResult(message)));
	}

	/// <summary>
	/// This method is called just after the test has finished running. By default, this sends
	/// <see cref="TestFinished"/>. Override this to enable any extensibility related to test
	/// finish.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// be reported as top-level exceptions. Any exceptions that are present in the aggregator (presumably
	/// from derived implementations of this method, <see cref="GetAttachments"/>, <see cref="GetTestOutput"/>,
	/// <see cref="GetWarnings"/>, or <see cref="RunTest"/>) will invoke <see cref="OnTestCleanupFailure"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="executionTime">The time spent running the test</param>
	/// <param name="output">The output from the test</param>
	/// <param name="warnings">The warnings that were generated during the test</param>
	/// <param name="attachments">The attachments that were assocated with the test</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual async ValueTask<bool> OnTestFinished(
		TContext ctxt,
		decimal executionTime,
		string output,
		string[]? warnings,
		IReadOnlyDictionary<string, TestAttachment>? attachments)
	{
		Guard.ArgumentNotNull(ctxt);

		if (ctxt.Aggregator.HasExceptions)
		{
			var exception = ctxt.Aggregator.ToException()!;
			ctxt.Aggregator.Clear();

			if (!await ctxt.Aggregator.RunAsync(() => OnTestCleanupFailure(ctxt, exception), true))
				ctxt.CancellationTokenSource.Cancel();
		}

		var result = ctxt.MessageBus.QueueMessage(new TestFinished
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			Attachments = attachments ?? TestFinished.EmptyAttachments,
			ExecutionTime = executionTime,
			FinishTime = DateTimeOffset.UtcNow,
			Output = output,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass?.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
			Warnings = warnings,
		});

		ctxt.Aggregator.Run(() => (TestContext.Current.TestOutputHelper as TestOutputHelper)?.Uninitialize());

		return result;
	}

	/// <summary>
	/// This method is called when a test was not run. By default, this sends <see cref="TestNotRun"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="output">The output from the test</param>
	/// <param name="warnings">The warnings that were generated during the test</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<(bool Continue, TestResultState ResultState)> OnTestNotRun(
		TContext ctxt,
		string output,
		string[]? warnings)
	{
		Guard.ArgumentNotNull(ctxt);

		var message = new TestNotRun
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			ExecutionTime = 0m,
			FinishTime = DateTimeOffset.UtcNow,
			Output = output,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass?.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
			Warnings = warnings,
		};

		return new((ctxt.MessageBus.QueueMessage(message), TestResultState.FromTestResult(message)));
	}

	/// <summary>
	/// This method is called when a test has passed. By default, this sends <see cref="TestPassed"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="executionTime">The time spent running the test</param>
	/// <param name="output">The output from the test</param>
	/// <param name="warnings">The warnings that were generated during the test</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<(bool Continue, TestResultState ResultState)> OnTestPassed(
		TContext ctxt,
		decimal executionTime,
		string output,
		string[]? warnings)
	{
		Guard.ArgumentNotNull(ctxt);

		var message = new TestPassed
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			ExecutionTime = executionTime,
			FinishTime = DateTimeOffset.UtcNow,
			Output = output,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass?.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
			Warnings = warnings,
		};

		return new((ctxt.MessageBus.QueueMessage(message), TestResultState.FromTestResult(message)));
	}

	/// <summary>
	/// This method is called when a test is skipped. By default, this sends <see cref="TestSkipped"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="skipReason">The reason given for skipping the test</param>
	/// <param name="executionTime">The time spent running the test</param>
	/// <param name="output">The output from the test</param>
	/// <param name="warnings">The warnings that were generated during the test</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<(bool Continue, TestResultState ResultState)> OnTestSkipped(
		TContext ctxt,
		string skipReason,
		decimal executionTime,
		string output,
		string[]? warnings)
	{
		Guard.ArgumentNotNull(ctxt);

		var message = new TestSkipped
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			ExecutionTime = executionTime,
			FinishTime = DateTimeOffset.UtcNow,
			Output = output,
			Reason = skipReason,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass?.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
			Warnings = warnings,
		};

		return new((ctxt.MessageBus.QueueMessage(message), TestResultState.FromTestResult(message)));
	}

	/// <summary>
	/// This method is called just before the test is run. By default, this sends
	/// <see cref="TestStarting"/>. Override this to enable any extensibility related to test
	/// start.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Initializing"/> and any exceptions thrown will
	/// contribute to test failure (and will prevent the test from running).  Even if this method records
	/// exceptions, <see cref="OnTestFinished"/> will be called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestStarting(TContext ctxt) =>
		OnTestStarting(ctxt, false, 0);

	/// <summary>
	/// This is a helper that allows passing explicit and timeout values, since those are not
	/// part of the core object model.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="explicit">A flag which indicates whether this is an explicit test</param>
	/// <param name="timeout">The timeout for running this test</param>
	protected ValueTask<bool> OnTestStarting(
		TContext ctxt,
		bool @explicit,
		int timeout)
	{
		Guard.ArgumentNotNull(ctxt);

		ctxt.Aggregator.Run(() => (TestContext.Current.TestOutputHelper as TestOutputHelper)?.Initialize(ctxt.MessageBus, ctxt.Test));

		return new(ctxt.MessageBus.QueueMessage(new TestStarting
		{
			AssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID,
			Explicit = @explicit,
			StartTime = DateTimeOffset.UtcNow,
			TestCaseUniqueID = ctxt.Test.TestCase.UniqueID,
			TestClassUniqueID = ctxt.Test.TestCase.TestClass?.UniqueID,
			TestCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID,
			TestDisplayName = ctxt.Test.TestDisplayName,
			TestMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID,
			TestUniqueID = ctxt.Test.UniqueID,
			Timeout = timeout,
			Traits = ctxt.Test.Traits,
		}));
	}

	/// <summary>
	/// Runs the test.
	/// </summary>
	/// <remarks>
	/// This function is the primary orchestrator of test execution.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	protected async ValueTask<RunSummary> Run(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		var summary = new RunSummary();
		var output = string.Empty;
		var warnings = default(string[]);
		var attachments = default(IReadOnlyDictionary<string, TestAttachment>);
		var elapsedTime = TimeSpan.Zero;

		if (!await ctxt.Aggregator.RunAsync(() => OnTestStarting(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		var @continue = true;
		TestResultState? resultState = null;

		SetTestContext(ctxt, TestEngineStatus.Running);

		if (!ctxt.CancellationTokenSource.IsCancellationRequested)
		{
			var shouldRun = true;
			if (!ctxt.Aggregator.HasExceptions)
				shouldRun = ctxt.Aggregator.Run(() => ShouldTestRun(ctxt), true);

			// When we don't pass an exception, we're looking for statically skipped test, so we
			// won't try to run something we suspect will fail
			var skipReason = ctxt.GetSkipReason(exception: null);

			if (!ctxt.Aggregator.HasExceptions && shouldRun && skipReason is null)
				elapsedTime += await ctxt.Aggregator.RunAsync(() => RunTest(ctxt), TimeSpan.Zero);

			output = await ctxt.Aggregator.RunAsync(() => GetTestOutput(ctxt), string.Empty);
			warnings = await ctxt.Aggregator.RunAsync(() => GetWarnings(ctxt), null);
			attachments = await ctxt.Aggregator.RunAsync(() => GetAttachments(ctxt), null);

			summary.Total = 1;
			summary.Time = (decimal)elapsedTime.TotalSeconds;

			var exception = ctxt.Aggregator.ToException();

			// We re-ask for skip reason to allow dynamic skip via exception, even though we
			// don't define what that means at this level. We let the context and/or derived
			// runner classes to provide that definition for us.
			skipReason = ctxt.GetSkipReason(exception);

			ctxt.Aggregator.Clear();

			if (!shouldRun)
			{
				summary.NotRun = 1;
				(@continue, resultState) = await ctxt.Aggregator.RunAsync(() => OnTestNotRun(ctxt, output, warnings), (true, TestResultState.ForNotRun()));
			}
			else if (skipReason is not null)
			{
				summary.Skipped = 1;
				(@continue, resultState) = await ctxt.Aggregator.RunAsync(() => OnTestSkipped(ctxt, skipReason, 0m, output, warnings), (true, TestResultState.ForNotRun()));
			}
			else if (exception is not null)
			{
				summary.Failed = 1;
				(@continue, resultState) = await ctxt.Aggregator.RunAsync(() => OnTestFailed(ctxt, exception, summary.Time, output, warnings), (true, TestResultState.ForNotRun()));
			}
			else
				(@continue, resultState) = await ctxt.Aggregator.RunAsync(() => OnTestPassed(ctxt, summary.Time, output, warnings), (true, TestResultState.ForNotRun()));
		}

		if (!@continue)
			ctxt.CancellationTokenSource.Cancel();

		SetTestContext(ctxt, TestEngineStatus.CleaningUp, resultState ?? TestResultState.ForNotRun());

		if (!await ctxt.Aggregator.RunAsync(() => OnTestFinished(ctxt, summary.Time, output, warnings, attachments), true))
			ctxt.CancellationTokenSource.Cancel();

		if (ctxt.Aggregator.HasExceptions)
			if (!await ctxt.Aggregator.RunAsync(() => OnError(ctxt, ctxt.Aggregator.ToException()!), true))
				ctxt.CancellationTokenSource.Cancel();

		ctxt.Aggregator.Clear();

		return summary;
	}

	/// <summary>
	/// Override this method to run the test.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	protected abstract ValueTask<TimeSpan> RunTest(TContext ctxt);

	/// <summary>
	/// Sets the test context for the given test state and engine status.
	/// </summary>
	/// <remarks>
	/// This method must never throw. Behavior is undefined if it does. Instead, exceptions that
	/// occur should be recorded in the aggregator in <paramref name="ctxt"/> and will be reflected
	/// in a way that's appropriate based on when this method is called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	/// <param name="testStatus">The current engine status for the test</param>
	/// <param name="testState">The current test state</param>
	/// <param name="testClassInstance">The instance of the test class</param>
	protected virtual void SetTestContext(
		TContext ctxt,
		TestEngineStatus testStatus,
		TestResultState? testState = null,
		object? testClassInstance = null)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTest(
			ctxt.Test,
			testStatus,
			ctxt.CancellationTokenSource.Token,
			testState,
			testStatus == TestEngineStatus.Initializing ? new TestOutputHelper() : TestContext.Current.TestOutputHelper,
			testClassInstance: testClassInstance
		);
	}

	/// <summary>
	/// Override this to determine whether a test should be run or not (meaning, if you return <c>false</c>,
	/// it will be reported with a status of <see cref="TestResult.NotRun"/>). By default, this method will
	/// return <c>true</c>. This is typically used to implement the ability to exclude specific tests
	/// unless they've been explicitly asked to be run.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test</param>
	protected virtual bool ShouldTestRun(TContext ctxt) =>
		true;

	/// <summary>
	/// Updates the test context values while the test is running, without swapping out the test context
	/// itself. This preserves the values in the existing context (notably, the cancellation token, which
	/// is wrapped and passed, and as such cannot be replaced).
	/// </summary>
	protected void UpdateTestContext(
		object? testClassInstance,
		TestResultState? testState = null)
	{
		TestContext.CurrentInternal.TestClassInstance = testClassInstance;

		if (testState is not null)
			TestContext.CurrentInternal.TestState = testState;
	}
}
