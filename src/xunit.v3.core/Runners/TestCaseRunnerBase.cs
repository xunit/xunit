using System;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running test cases.
/// </summary>
/// <remarks>
/// This class does not make any test-related assumptions about test cases, only that
/// at some point, a test case will be "run" and results will be provided. As such, it
/// has no definitions that related to tests (or <see cref="ITest"/>). The intention with
/// this base class is that it can serve as a base for non-traditional test cases, such
/// as injecting errors into the test pipeline during discovery that aren't uncovered
/// until execution time.
/// </remarks>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
public abstract class TestCaseRunnerBase<TContext, TTestCase>
	where TContext : TestCaseRunnerBaseContext<TTestCase>
	where TTestCase : class, ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestCaseRunnerBase{TContext, TTestCase}"/> class.
	/// </summary>
	protected TestCaseRunnerBase()
	{ }

	/// <summary>
	/// This method is called when an exception was thrown by <see cref="OnTestCaseFinished"/>. By default, this
	/// sends <see cref="ErrorMessage"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/>. It must never throw an exception.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="exception">The exception that was thrown by <see cref="OnTestCaseFinished"/>.</param>
	protected virtual ValueTask<bool> OnError(
		TContext ctxt,
		Exception exception) =>
			new(Guard.ArgumentNotNull(ctxt).MessageBus.QueueMessage(ErrorMessage.FromException(exception)));

	/// <summary>
	/// This method is called when an exception was thrown while cleaning up, after the test
	/// case has run. By default, this sends <see cref="TestCaseCleanupFailure"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown are
	/// converted into fatal exception messages (via <see cref="IErrorMessage"/>) and sent to the message
	/// bus in <paramref name="ctxt"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test case</param>
	/// <param name="exception">The exception that caused the cleanup failure (may be an instance
	/// of <see cref="AggregateException"/> if more than one exception occurred).</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestCaseCleanupFailure(
		TContext ctxt,
		Exception exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var (types, messages, stackTraces, indices, _) = ExceptionUtility.ExtractMetadata(exception);

		return new(ctxt.MessageBus.QueueMessage(new TestCaseCleanupFailure
		{
			AssemblyUniqueID = ctxt.TestCase.TestCollection.TestAssembly.UniqueID,
			ExceptionParentIndices = indices,
			ExceptionTypes = types,
			Messages = messages,
			StackTraces = stackTraces,
			TestCaseUniqueID = ctxt.TestCase.UniqueID,
			TestClassUniqueID = ctxt.TestCase.TestClass?.UniqueID,
			TestCollectionUniqueID = ctxt.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.TestCase.TestMethod?.UniqueID,
		}));
	}

	/// <summary>
	/// This method will be called when the test case has finished running. By default, this sends
	/// <see cref="TestCaseFinished"/>. Override this to enable any extensibility related to test
	/// case finish.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// be reported as top-level exceptions. Any exceptions that are present in the aggregator (presumably
	/// from derived implementations of this method or <see cref="RunTestCase"/>) will invoke
	/// <see cref="OnTestCaseCleanupFailure"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test case</param>
	/// <param name="summary">The execution summary for the test case.</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual async ValueTask<bool> OnTestCaseFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		if (ctxt.Aggregator.HasExceptions)
		{
			var exception = ctxt.Aggregator.ToException()!;
			ctxt.Aggregator.Clear();

			if (!await ctxt.Aggregator.RunAsync(() => OnTestCaseCleanupFailure(ctxt, exception), true))
				ctxt.CancellationTokenSource.Cancel();
		}

		return ctxt.MessageBus.QueueMessage(new TestCaseFinished
		{
			AssemblyUniqueID = ctxt.TestCase.TestCollection.TestAssembly.UniqueID,
			ExecutionTime = summary.Time,
			TestCaseUniqueID = ctxt.TestCase.UniqueID,
			TestClassUniqueID = ctxt.TestCase.TestClass?.UniqueID,
			TestCollectionUniqueID = ctxt.TestCase.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.TestCase.TestMethod?.UniqueID,
			TestsFailed = summary.Failed,
			TestsNotRun = summary.NotRun,
			TestsSkipped = summary.Skipped,
			TestsTotal = summary.Total,
		});
	}

	/// <summary>
	/// This method will be called before the test case has started running. TBy default, this sends
	/// <see cref="TestCaseStarting"/>. Override this to enable any extensibility related to test
	/// case start.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Initializing"/> and any exceptions thrown will
	/// contribute to test case failure (and will prevent the test case from running). Even if
	/// this method records exceptions, <see cref="OnTestCaseFinished"/> will be called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test case</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestCaseStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestCaseStarting
		{
			AssemblyUniqueID = ctxt.TestCase.TestCollection.TestAssembly.UniqueID,
			Explicit = ctxt.TestCase.Explicit,
			SkipReason = Guard.ArgumentNotNull(ctxt).TestCase.SkipReason,
			SourceFilePath = ctxt.TestCase.SourceFilePath,
			SourceLineNumber = ctxt.TestCase.SourceLineNumber,
			TestCaseDisplayName = ctxt.TestCase.TestCaseDisplayName,
			TestCaseUniqueID = ctxt.TestCase.UniqueID,
			TestClassMetadataToken = ctxt.TestCase.TestClassMetadataToken,
			TestClassName = ctxt.TestCase.TestClassName,
			TestClassNamespace = ctxt.TestCase.TestClassNamespace,
			TestClassSimpleName = ctxt.TestCase.TestClassSimpleName,
			TestClassUniqueID = ctxt.TestCase.TestClass?.UniqueID,
			TestCollectionUniqueID = ctxt.TestCase.TestCollection.UniqueID,
			TestMethodMetadataToken = ctxt.TestCase.TestMethodMetadataToken,
			TestMethodName = ctxt.TestCase.TestMethodName,
			TestMethodParameterTypesVSTest = ctxt.TestCase.TestMethodParameterTypesVSTest,
			TestMethodReturnTypeVSTest = ctxt.TestCase.TestMethodReturnTypeVSTest,
			TestMethodUniqueID = ctxt.TestCase.TestMethod?.UniqueID,
			Traits = ctxt.TestCase.Traits,
		}));
	}

	/// <summary>
	/// Executes the administrivia around running a test case, while leaving the actual
	/// test case execution up to <see cref="RunTestCase"/>.
	/// </summary>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected async ValueTask<RunSummary> Run(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		var summary = default(RunSummary);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestCaseStarting(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		SetTestContext(ctxt, TestEngineStatus.Running);

		var startupException = ctxt.Aggregator.ToException();
		ctxt.Aggregator.Clear();

		if (!ctxt.CancellationTokenSource.IsCancellationRequested)
			summary = await ctxt.Aggregator.RunAsync(() => RunTestCase(ctxt, startupException), default);

		SetTestContext(ctxt, TestEngineStatus.CleaningUp);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestCaseFinished(ctxt, summary), true))
			ctxt.CancellationTokenSource.Cancel();

		if (ctxt.Aggregator.HasExceptions)
			if (!await ctxt.Aggregator.RunAsync(() => OnError(ctxt, ctxt.Aggregator.ToException()!), true))
				ctxt.CancellationTokenSource.Cancel();

		ctxt.Aggregator.Clear();

		return summary;
	}

	/// <summary>
	/// Override this to run the test case.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test case cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test case</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected abstract ValueTask<RunSummary> RunTestCase(
		TContext ctxt,
		Exception? exception);

	/// <summary>
	/// Sets the current <see cref="TestContext"/> for the current test case and the given test case status.
	/// </summary>
	/// <remarks>
	/// This method must never throw. Behavior is undefined if it does. Instead, exceptions that
	/// occur should be recorded in the aggregator in <paramref name="ctxt"/> and will be reflected
	/// in a way that's appropriate based on when this method is called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test case</param>
	/// <param name="testCaseStatus">The current test case status.</param>
	protected virtual void SetTestContext(
		TContext ctxt,
		TestEngineStatus testCaseStatus)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTestCase(ctxt.TestCase, testCaseStatus, ctxt.CancellationTokenSource.Token);
	}
}
