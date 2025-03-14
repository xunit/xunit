using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running tests in an assembly. It groups the tests
/// by test collection, and then runs the individual test collections.
/// </summary>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTestAssembly">The type of the test assembly object model. Must derive
/// from <see cref="ITestAssembly"/>.</typeparam>
/// <typeparam name="TTestCollection">The type of the test collection object model. Must derive
/// from <see cref="ITestCollection"/>.</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ITestCase"/>.</typeparam>
public abstract class TestAssemblyRunner<TContext, TTestAssembly, TTestCollection, TTestCase>
	where TContext : TestAssemblyRunnerContext<TTestAssembly, TTestCase>
	where TTestAssembly : class, ITestAssembly
	where TTestCollection : class, ITestCollection
	where TTestCase : class, ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestAssemblyRunner{TContext, TTestAssembly, TTestCollection, TTestCase}"/> class.
	/// </summary>
	protected TestAssemblyRunner()
	{ }

	/// <summary>
	/// Fails the tests from a test collection due to an exception.
	/// </summary>
	/// <remarks>
	/// By default, using <see cref="XunitRunnerHelper"/> to fail the test cases.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <param name="testCollection">The test collection that is being failed.</param>
	/// <param name="testCases">The test cases that belong to the test collection.</param>
	/// <param name="exception">The exception that was caused during startup.</param>
	/// <returns>Returns summary information about the tests that were failed.</returns>
	protected virtual ValueTask<RunSummary> FailTestCollection(
		TContext ctxt,
		TTestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases,
		Exception exception) =>
			new(XunitRunnerHelper.FailTestCases(
				Guard.ArgumentNotNull(ctxt).MessageBus,
				ctxt.CancellationTokenSource,
				testCases,
				exception,
				sendTestCollectionMessages: true,
				sendTestClassMessages: true,
				sendTestMethodMessages: true
			));

	/// <summary>
	/// Gets the display name for the test framework. Used to populate <see cref="TestAssemblyStarting"/>
	/// during <see cref="OnTestAssemblyStarting"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	protected abstract ValueTask<string> GetTestFrameworkDisplayName(TContext ctxt);

	/// <summary>
	/// This method is called when an exception was thrown by <see cref="OnTestAssemblyFinished"/>. By default, this
	/// sends <see cref="ErrorMessage"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/>. It must never throw an exception.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="exception">The exception that was thrown by <see cref="OnTestAssemblyFinished"/>.</param>
	protected virtual ValueTask<bool> OnError(
		TContext ctxt,
		Exception exception) =>
			new(Guard.ArgumentNotNull(ctxt).MessageBus.QueueMessage(ErrorMessage.FromException(exception)));

	/// <summary>
	/// This method is called when an exception was thrown while cleaning up, after the test assembly
	/// has run. By default this sends <see cref="TestAssemblyCleanupFailure"/>.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown are
	/// converted into fatal exception messages (via <see cref="IErrorMessage"/>) and sent to the message
	/// bus in <paramref name="ctxt"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <param name="exception">The exception that caused the cleanup failure (may be an instance
	/// of <see cref="AggregateException"/> if more than one exception occurred).</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual ValueTask<bool> OnTestAssemblyCleanupFailure(
		TContext ctxt,
		Exception exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var (types, messages, stackTraces, indices, _) = ExceptionUtility.ExtractMetadata(exception);

		return new(ctxt.MessageBus.QueueMessage(new TestAssemblyCleanupFailure
		{
			AssemblyUniqueID = ctxt.TestAssembly.UniqueID,
			ExceptionParentIndices = indices,
			ExceptionTypes = types,
			Messages = messages,
			StackTraces = stackTraces,
		}));
	}

	/// <summary>
	/// This method will be called when the test assembly has finished running. By default this sends
	/// <see cref="TestAssemblyFinished"/>. Override this to enable any extensibility related to test
	/// assembly finish.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.CleaningUp"/> and any exceptions thrown will
	/// be reported as top-level exceptions. Any exceptions that are present in the aggregator (presumably
	/// from derived implementations of this method, <see cref="RunTestCollections"/>, or
	/// <see cref="RunTestCollection"/>) will invoke <see cref="OnTestAssemblyCleanupFailure"/>.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <param name="summary">The execution summary for the test assembly</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual async ValueTask<bool> OnTestAssemblyFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		if (ctxt.Aggregator.HasExceptions)
		{
			var exception = ctxt.Aggregator.ToException()!;
			ctxt.Aggregator.Clear();

			if (!await ctxt.Aggregator.RunAsync(() => OnTestAssemblyCleanupFailure(ctxt, exception), true))
				ctxt.CancellationTokenSource.Cancel();
		}

		return ctxt.MessageBus.QueueMessage(new TestAssemblyFinished
		{
			AssemblyUniqueID = ctxt.TestAssembly.UniqueID,
			FinishTime = DateTimeOffset.Now,
			ExecutionTime = summary.Time,
			TestsFailed = summary.Failed,
			TestsNotRun = summary.NotRun,
			TestsSkipped = summary.Skipped,
			TestsTotal = summary.Total,
		});
	}

	/// <summary>
	/// This method will be called before the test assembly has started running. TBy default this sends
	/// <see cref="TestAssemblyStarting"/>. Override this to enable any extensibility related to test
	/// assembly start.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Initializing"/> and any exceptions thrown will
	/// contribute to test assembly failure (and will prevent the test assembly from running). Even if this
	/// method records exceptions, <see cref="OnTestAssemblyFinished"/> will be called.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <returns>Return <c>true</c> if test execution should continue; <c>false</c> if it should be shut down.</returns>
	protected virtual async ValueTask<bool> OnTestAssemblyStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return ctxt.MessageBus.QueueMessage(new TestAssemblyStarting
		{
			AssemblyName = Path.GetFileNameWithoutExtension(ctxt.TestAssembly.AssemblyPath),
			AssemblyPath = ctxt.TestAssembly.AssemblyPath,
			AssemblyUniqueID = ctxt.TestAssembly.UniqueID,
			ConfigFilePath = ctxt.TestAssembly.ConfigFilePath,
			Seed = Randomizer.Seed,
			StartTime = DateTimeOffset.Now,
			TargetFramework = ctxt.TargetFramework,
			TestEnvironment = ctxt.TestEnvironment,
			TestFrameworkDisplayName = await GetTestFrameworkDisplayName(ctxt),
			Traits = ctxt.TestAssembly.Traits,
		});
	}

	/// <summary>
	/// Orders the test collections in the assembly. By default does not re-order the test collections.
	/// Override this to provide custom test collection ordering.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <returns>Test collections in run order (and associated, not-yet-ordered test cases).</returns>
	protected virtual List<(TTestCollection Collection, List<TTestCase> TestCases)> OrderTestCollections(TContext ctxt) =>
		OrderTestCollectionsDefault(ctxt);

	static List<(TTestCollection Collection, List<TTestCase> TestCases)> OrderTestCollectionsDefault(TContext ctxt) =>
		Guard.ArgumentNotNull(ctxt)
			.TestCases
			.GroupBy(tc => (TTestCollection)tc.TestCollection, TestCollectionComparer<TTestCollection>.Instance)
			.Select(tc => (Collection: tc.Key, TestCases: tc.ToList()))
			.ToList();

	/// <summary>
	/// Runs the tests in the test assembly.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected async ValueTask<RunSummary> Run(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		// We eventually want clock time, not aggregated run time
		var clockTimeStopwatch = Stopwatch.StartNew();
		var summary = default(RunSummary);

		if (!await ctxt.Aggregator.RunAsync(() => OnTestAssemblyStarting(ctxt), true))
			ctxt.CancellationTokenSource.Cancel();

		SetTestContext(ctxt, TestEngineStatus.Running);

		var startingException = ctxt.Aggregator.ToException();
		ctxt.Aggregator.Clear();

		if (!ctxt.CancellationTokenSource.IsCancellationRequested)
			summary = await ctxt.Aggregator.RunAsync(() => RunTestCollections(ctxt, startingException), default);

		SetTestContext(ctxt, TestEngineStatus.CleaningUp);

		summary.Time = (decimal)clockTimeStopwatch.Elapsed.TotalSeconds;

		if (!await ctxt.Aggregator.RunAsync(() => OnTestAssemblyFinished(ctxt, summary), true))
			ctxt.CancellationTokenSource.Cancel();

		if (ctxt.Aggregator.HasExceptions)
			if (!await ctxt.Aggregator.RunAsync(() => OnError(ctxt, ctxt.Aggregator.ToException()!), true))
				ctxt.CancellationTokenSource.Cancel();

		ctxt.Aggregator.Clear();

		return summary;
	}

	/// <summary>
	/// Runs the list of test collections. By default, groups the tests by collection and runs them synchronously.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test assembly cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected virtual async ValueTask<RunSummary> RunTestCollections(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var summary = new RunSummary();
		var orderedCollections = exception is null ? OrderTestCollections(ctxt) : OrderTestCollectionsDefault(ctxt);

		foreach (var collection in orderedCollections)
		{
			if (exception is not null)
				summary.Aggregate(await FailTestCollection(ctxt, collection.Collection, collection.TestCases, exception));
			else
				summary.Aggregate(await RunTestCollection(ctxt, collection.Collection, collection.TestCases));

			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				break;
		}

		return summary;
	}

	/// <summary>
	/// Override this method to run the tests in an individual test collection.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test assembly cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <param name="testCollection">The test collection that is being run.</param>
	/// <param name="testCases">The test cases that belong to the test collection.</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected abstract ValueTask<RunSummary> RunTestCollection(
		TContext ctxt,
		TTestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases
	);

	/// <summary>
	/// Sets the current <see cref="TestContext"/> for the current test assembly and the given test assembly status.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <param name="testAssemblyStatus">The current test assembly status.</param>
	protected virtual void SetTestContext(
		TContext ctxt,
		TestEngineStatus testAssemblyStatus)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTestAssembly(ctxt.TestAssembly, testAssemblyStatus, ctxt.CancellationTokenSource.Token);
	}
}
