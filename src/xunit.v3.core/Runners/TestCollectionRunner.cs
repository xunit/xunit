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
	/// This method is called when an exception was thrown while cleaning up, after the test collection
	/// has run. This will typically send a "test collection cleanup failure" message (like
	/// <see cref="ITestCollectionCleanupFailure"/>).
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
	protected abstract ValueTask<bool> OnTestCollectionCleanupFailure(
		TContext ctxt,
		Exception exception);

	/// <summary>
	/// This method will be called when the test collection has finished running. This will typically send
	/// a "test collection finished" message (like <see cref="ITestCollectionFinished"/>) as well as enabling
	/// any extensibility related to test collection finish.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// contribute to test collection cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <param name="summary">The execution summary for the test collection</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestCollectionFinished(
		TContext ctxt,
		RunSummary summary);

	/// <summary>
	/// This method will be called before the test collection has started running. This will typically send
	/// a "test collection starting" message (like <see cref="ITestCollectionStarting"/>) as well as enabling
	/// any extensibility related to test collection start.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Initializing"/> and any exceptions thrown will
	/// contribute to test collection failure (and will prevent the test collection from running). Even if
	/// this method records exceptions, <see cref="OnTestCollectionFinished"/> will be called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected abstract ValueTask<bool> OnTestCollectionStarting(TContext ctxt);

	/// <summary>
	/// Runs the tests in the test collection.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected async ValueTask<RunSummary> RunAsync(TContext ctxt)
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
			summary = await ctxt.Aggregator.RunAsync(() => RunTestClassesAsync(ctxt, startingException), default);

		SetTestContext(ctxt, TestEngineStatus.CleaningUp);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestCollectionFinished(ctxt, summary), true))
			ctxt.CancellationTokenSource.Cancel();

		if (ctxt.Aggregator.HasExceptions)
		{
			var exception = ctxt.Aggregator.ToException()!;
			ctxt.Aggregator.Clear();

			if (!await ctxt.Aggregator.RunAsync(() => OnTestCollectionCleanupFailure(ctxt, exception), true))
				ctxt.CancellationTokenSource.Cancel();

			if (ctxt.Aggregator.HasExceptions)
				ctxt.MessageBus.QueueMessage(ErrorMessage.FromException(ctxt.Aggregator.ToException()!));

			ctxt.Aggregator.Clear();
		}

		return summary;
	}

	/// <summary>
	/// Runs the list of test classes. By default, groups the tests by class and runs them synchronously.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected virtual async ValueTask<RunSummary> RunTestClassesAsync(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var summary = new RunSummary();

		foreach (var testCasesByClass in ctxt.TestCases.GroupBy(tc => tc.TestClass, TestClassComparer.Instance))
		{
			summary.Aggregate(await RunTestClassAsync(ctxt, testCasesByClass.Key as TTestClass, testCasesByClass.CastOrToReadOnlyCollection(), exception));
			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				break;
		}

		return summary;
	}

	/// <summary>
	/// Override this method to run the tests in an individual test class.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <param name="testClass">The test class to be run. May be <c>null</c> for test cases that do not
	/// support classes and methods.</param>
	/// <param name="testCases">The test cases to be run.</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected abstract ValueTask<RunSummary> RunTestClassAsync(
		TContext ctxt,
		TTestClass? testClass,
		IReadOnlyCollection<TTestCase> testCases,
		Exception? exception
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
