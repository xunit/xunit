using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running tests in a test collection. It groups the tests
/// by test class, and then runs the individual test classes.
/// </summary>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTestCollection">The type of the test collection used by the test framework.
/// Must derive from <see cref="ITestCollection"/>.</typeparam>
/// <typeparam name="TTestClass">The type of the test class used by the test framework.
/// Must derive from <see cref="ITestClass"/>.</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
public abstract class TestCollectionRunner<TContext, TTestCollection, TTestClass, TTestCase>
	where TContext : TestCollectionRunnerContext<TTestCollection, TTestCase>
	where TTestCollection : class, ITestCollection
	where TTestClass : class, ITestClass
	where TTestCase : class, ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestCollectionRunner{TContext, TTestCollection, TTestClass, TTestCase}"/> class.
	/// </summary>
	protected TestCollectionRunner()
	{ }

	/// <summary>
	/// Fails the tests from a test class due to an exception.
	/// </summary>
	/// <remarks>
	/// By default, using <see cref="XunitRunnerHelper"/> to fail the test cases.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <param name="testClass">The test class that is being failed. May be <c>null</c> for test cases that do not
	/// support classes and methods.</param>
	/// <param name="testCases">The test cases to be failed.</param>
	/// <param name="exception">The exception that was caused during startup.</param>
	/// <returns>Returns summary information about the tests that were failed.</returns>
	protected virtual ValueTask<RunSummary> FailTestClass(
		TContext ctxt,
		TTestClass? testClass,
		IReadOnlyCollection<TTestCase> testCases,
		Exception exception) =>
			new(XunitRunnerHelper.FailTestCases(
				Guard.ArgumentNotNull(ctxt).MessageBus,
				ctxt.CancellationTokenSource,
				testCases,
				exception,
				sendTestClassMessages: true,
				sendTestMethodMessages: true
			));

	/// <summary>
	/// This method is called when an exception was thrown by <see cref="OnTestCollectionFinished"/>. By default, this
	/// sends <see cref="ErrorMessage"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/>. It must never throw an exception.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="exception">The exception that was thrown by <see cref="OnTestCollectionFinished"/>.</param>
	protected virtual ValueTask<bool> OnError(
		TContext ctxt,
		Exception exception) =>
			new(Guard.ArgumentNotNull(ctxt).MessageBus.QueueMessage(ErrorMessage.FromException(exception)));

	/// <summary>
	/// This method is called when an exception was thrown while cleaning up, after the test collection
	/// has run. By default, this sends <see cref="TestCollectionCleanupFailure"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown are
	/// converted into fatal exception messages (via <see cref="IErrorMessage"/>) and sent to the message
	/// bus in <paramref name="ctxt"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <param name="exception">The exception that caused the cleanup failure (may be an instance
	/// of <see cref="AggregateException"/> if more than one exception occurred).</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestCollectionCleanupFailure(
		TContext ctxt,
		Exception exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var (types, messages, stackTraces, indices, _) = ExceptionUtility.ExtractMetadata(exception);

		return new(ctxt.MessageBus.QueueMessage(new TestCollectionCleanupFailure
		{
			AssemblyUniqueID = ctxt.TestCollection.TestAssembly.UniqueID,
			ExceptionParentIndices = indices,
			ExceptionTypes = types,
			Messages = messages,
			StackTraces = stackTraces,
			TestCollectionUniqueID = ctxt.TestCollection.UniqueID,
		}));
	}

	/// <summary>
	/// This method will be called when the test collection has finished running. By default this sends
	/// <see cref="TestCollectionFinished"/>. Override this to enable any extensibility related to test
	/// collection finish.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// be reported as top-level exceptions. Any exceptions that are present in the aggregator (presumably
	/// from derived implementations of this method, <see cref="RunTestClasses"/>, or <see cref="RunTestClass"/>)
	/// will invoke <see cref="OnTestCollectionCleanupFailure"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <param name="summary">The execution summary for the test collection</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual async ValueTask<bool> OnTestCollectionFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		if (ctxt.Aggregator.HasExceptions)
		{
			var exception = ctxt.Aggregator.ToException()!;
			ctxt.Aggregator.Clear();

			if (!await ctxt.Aggregator.RunAsync(() => OnTestCollectionCleanupFailure(ctxt, exception), true))
				ctxt.CancellationTokenSource.Cancel();
		}

		return ctxt.MessageBus.QueueMessage(new TestCollectionFinished
		{
			AssemblyUniqueID = ctxt.TestCollection.TestAssembly.UniqueID,
			ExecutionTime = summary.Time,
			TestCollectionUniqueID = ctxt.TestCollection.UniqueID,
			TestsFailed = summary.Failed,
			TestsNotRun = summary.NotRun,
			TestsSkipped = summary.Skipped,
			TestsTotal = summary.Total,
		});
	}

	/// <summary>
	/// This method will be called before the test collection has started running. By default this sends
	/// <see cref="TestCollectionStarting"/>. Override this to enable any extensibility related to test
	/// collection start.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Initializing"/> and any exceptions thrown will
	/// contribute to test collection failure (and will prevent the test collection from running). Even if
	/// this method records exceptions, <see cref="OnTestCollectionFinished"/> will be called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestCollectionStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestCollectionStarting
		{
			AssemblyUniqueID = Guard.ArgumentNotNull(ctxt).TestCollection.TestAssembly.UniqueID,
			TestCollectionClassName = ctxt.TestCollection.TestCollectionClassName,
			TestCollectionDisplayName = ctxt.TestCollection.TestCollectionDisplayName,
			TestCollectionUniqueID = ctxt.TestCollection.UniqueID,
			Traits = ctxt.TestCollection.Traits,
		}));
	}

	/// <summary>
	/// Runs the tests in the test collection.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected async ValueTask<RunSummary> Run(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		var summary = default(RunSummary);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestCollectionStarting(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		SetTestContext(ctxt, TestEngineStatus.Running);

		var startingException = ctxt.Aggregator.ToException();
		ctxt.Aggregator.Clear();

		if (!ctxt.CancellationTokenSource.IsCancellationRequested)
			summary = await ctxt.Aggregator.RunAsync(() => RunTestClasses(ctxt, startingException), default);

		SetTestContext(ctxt, TestEngineStatus.CleaningUp);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestCollectionFinished(ctxt, summary), true))
			ctxt.CancellationTokenSource.Cancel();

		if (ctxt.Aggregator.HasExceptions)
			if (!await ctxt.Aggregator.RunAsync(() => OnError(ctxt, ctxt.Aggregator.ToException()!), true))
				ctxt.CancellationTokenSource.Cancel();

		ctxt.Aggregator.Clear();

		return summary;
	}

	/// <summary>
	/// Runs the list of test classes. By default, groups the tests by class and runs them synchronously.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test collection cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected virtual async ValueTask<RunSummary> RunTestClasses(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var summary = new RunSummary();

		foreach (var testCasesByClass in ctxt.TestCases.GroupBy(tc => tc.TestClass, TestClassComparer.Instance))
		{
			var testClass = testCasesByClass.Key as TTestClass;
			var testCases = testCasesByClass.CastOrToReadOnlyCollection();

			if (exception is not null)
				summary.Aggregate(await FailTestClass(ctxt, testClass, testCases, exception));
			else
				summary.Aggregate(await RunTestClass(ctxt, testClass, testCases));

			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				break;
		}

		return summary;
	}

	/// <summary>
	/// Override this method to run the tests in an individual test class.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test collection cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <param name="testClass">The test class to be run. May be <c>null</c> for test cases that do not
	/// support classes and methods.</param>
	/// <param name="testCases">The test cases to be run.</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected abstract ValueTask<RunSummary> RunTestClass(
		TContext ctxt,
		TTestClass? testClass,
		IReadOnlyCollection<TTestCase> testCases
	);

	/// <summary>
	/// Sets the current <see cref="TestContext"/> for the current test collection and the given test collection status.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <param name="testCollectionStatus">The current test collection status.</param>
	protected virtual void SetTestContext(
		TContext ctxt,
		TestEngineStatus testCollectionStatus)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTestCollection(ctxt.TestCollection, testCollectionStatus, ctxt.CancellationTokenSource.Token);
	}
}
