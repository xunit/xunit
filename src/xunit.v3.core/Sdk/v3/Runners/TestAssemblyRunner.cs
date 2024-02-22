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
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="_ITestCase"/>.</typeparam>
public abstract class TestAssemblyRunner<TContext, TTestCase>
	where TContext : TestAssemblyRunnerContext<TTestCase>
	where TTestCase : _ITestCase
{
	static readonly Lazy<ITestCaseOrderer> defaultTestCaseOrderer = new(() => new DefaultTestCaseOrderer());
	static readonly Lazy<ITestCollectionOrderer> defaultTestCollectionOrderer = new(() => new DefaultTestCollectionOrderer());

	/// <summary>
	/// Initializes a new instance of the <see cref="TestAssemblyRunner{TContext, TTestCase}"/> class.
	/// </summary>
	protected TestAssemblyRunner()
	{ }

	/// <summary>
	/// This method is called just after <see cref="_TestAssemblyStarting"/> is sent, but before any test collections are run.
	/// This method should NEVER throw; any exceptions should be placed into the Aggregator in <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	protected virtual ValueTask AfterTestAssemblyStartingAsync(TContext ctxt) =>
		default;

	/// <summary>
	/// This method is called just before <see cref="_TestAssemblyFinished"/> is sent, but after all test collections are run.
	/// This method should NEVER throw; any exceptions should be placed into the Aggregator in <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	protected virtual ValueTask BeforeTestAssemblyFinishedAsync(TContext ctxt) =>
		default;

	/// <summary>
	/// Override this to provide a default test case orderer for use when ordering tests in test collections
	/// and test classes. Defaults to an instance of <see cref="DefaultTestCaseOrderer"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	protected virtual ITestCaseOrderer GetTestCaseOrderer(TContext ctxt) =>
		defaultTestCaseOrderer.Value;

	/// <summary>
	/// Orderride this to provide the default test collection order for ordering collections in the assembly.
	/// Defaults to an instance of <see cref="DefaultTestCollectionOrderer"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	protected virtual ITestCollectionOrderer GetTestCollectionOrderer(TContext ctxt) =>
		defaultTestCollectionOrderer.Value;

	/// <summary>
	/// Orders the test collections in the assembly.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <returns>Test collections in run order (and associated, not-yet-ordered test cases).</returns>
	protected List<Tuple<_ITestCollection, List<TTestCase>>> OrderTestCollections(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var testCollectionOrderer = GetTestCollectionOrderer(ctxt);
		var testCasesByCollection =
			ctxt.TestCases
				.GroupBy(tc => tc.TestCollection, TestCollectionComparer.Instance)
				.ToDictionary(collectionGroup => collectionGroup.Key, collectionGroup => collectionGroup.ToList());

		IReadOnlyCollection<_ITestCollection> orderedTestCollections;

		try
		{
			orderedTestCollections = testCollectionOrderer.OrderTestCollections(testCasesByCollection.Keys);
		}
		catch (Exception ex)
		{
			var innerEx = ex.Unwrap();

			TestContext.Current?.SendDiagnosticMessage(
				"Test collection orderer '{0}' threw '{1}' during ordering: {2}{3}{4}",
				testCollectionOrderer.GetType().FullName,
				innerEx.GetType().FullName,
				innerEx.Message,
				Environment.NewLine,
				innerEx.StackTrace
			);

			orderedTestCollections = testCasesByCollection.Keys.CastOrToReadOnlyCollection();
		}

		return
			orderedTestCollections
				.Select(collection => Tuple.Create(collection, testCasesByCollection[collection]))
				.ToList();
	}

	/// <summary>
	/// Runs the tests in the test assembly.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected async ValueTask<RunSummary> RunAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		var totalSummary = new RunSummary();

		try
		{
			var assemblyFolder = Path.GetDirectoryName(ctxt.TestAssembly.Assembly.AssemblyPath);
			if (assemblyFolder is not null)
				Directory.SetCurrentDirectory(assemblyFolder);
		}
		catch { }

		var testAssemblyStartingMessage = new _TestAssemblyStarting
		{
			AssemblyName = ctxt.TestAssembly.Assembly.Name,
			AssemblyPath = ctxt.TestAssembly.Assembly.AssemblyPath,
			AssemblyUniqueID = ctxt.TestAssembly.UniqueID,
			ConfigFilePath = ctxt.TestAssembly.ConfigFileName,
			Seed = ctxt.Seed,
			StartTime = DateTimeOffset.Now,
			TargetFramework = ctxt.TargetFramework,
			TestEnvironment = ctxt.TestFrameworkEnvironment,
			TestFrameworkDisplayName = ctxt.TestFrameworkDisplayName,
		};

		if (ctxt.MessageBus.QueueMessage(testAssemblyStartingMessage))
		{
			try
			{
				await AfterTestAssemblyStartingAsync(ctxt);

				SetTestContext(ctxt, TestEngineStatus.Running);

				// Want clock time, not aggregated run time
				var clockTimeStopwatch = Stopwatch.StartNew();
				totalSummary = await RunTestCollectionsAsync(ctxt);
				totalSummary.Time = (decimal)clockTimeStopwatch.Elapsed.TotalSeconds;

				SetTestContext(ctxt, TestEngineStatus.CleaningUp);

				ctxt.Aggregator.Clear();
				await BeforeTestAssemblyFinishedAsync(ctxt);

				if (ctxt.Aggregator.HasExceptions)
				{
					var cleanupFailure = _TestAssemblyCleanupFailure.FromException(ctxt.Aggregator.ToException()!, ctxt.TestAssembly.UniqueID);
					ctxt.MessageBus.QueueMessage(cleanupFailure);
				}
			}
			finally
			{
				var assemblyFinished = new _TestAssemblyFinished
				{
					AssemblyUniqueID = ctxt.TestAssembly.UniqueID,
					ExecutionTime = totalSummary.Time,
					FinishTime = DateTimeOffset.Now,
					TestsFailed = totalSummary.Failed,
					TestsNotRun = totalSummary.NotRun,
					TestsSkipped = totalSummary.Skipped,
					TestsTotal = totalSummary.Total,
				};

				ctxt.MessageBus.QueueMessage(assemblyFinished);
			}
		}

		return totalSummary;
	}

	/// <summary>
	/// Runs the list of test collections. By default, groups the tests by collection and runs them synchronously.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected virtual async ValueTask<RunSummary> RunTestCollectionsAsync(TContext ctxt)
	{
		var summary = new RunSummary();

		foreach (var collection in OrderTestCollections(ctxt))
		{
			summary.Aggregate(await RunTestCollectionAsync(ctxt, collection.Item1, collection.Item2));
			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				break;
		}

		return summary;
	}

	/// <summary>
	/// Override this method to run the tests in an individual test collection.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <param name="testCollection">The test collection that is being run.</param>
	/// <param name="testCases">The test cases that belong to the test collection.</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected abstract ValueTask<RunSummary> RunTestCollectionAsync(
		TContext ctxt,
		_ITestCollection testCollection,
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
