using System.Collections.Generic;
using System.Globalization;
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
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="_ITestCase"/>.</typeparam>
public abstract class TestCollectionRunner<TContext, TTestCase>
	where TContext : TestCollectionRunnerContext<TTestCase>
	where TTestCase : class, _ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestCollectionRunner{TContext, TTestCase}"/> class.
	/// </summary>
	protected TestCollectionRunner()
	{ }

	/// <summary>
	/// This method is called just after <see cref="_TestCollectionStarting"/> is sent, but before any test classes are run.
	/// This method should NEVER throw; any exceptions should be placed into the Aggregator in <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test collection</param>
	protected virtual ValueTask AfterTestCollectionStartingAsync(TContext ctxt)
		=> default;

	/// <summary>
	/// This method is called just before <see cref="_TestCollectionFinished"/> is sent, but after all test classes have run.
	/// This method should NEVER throw; any exceptions should be placed into the Aggregator in <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test collection</param>
	protected virtual ValueTask BeforeTestCollectionFinishedAsync(TContext ctxt)
		=> default;

	/// <summary>
	/// Runs the tests in the test collection.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected async ValueTask<RunSummary> RunAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		var collectionSummary = new RunSummary();
		var testAssemblyUniqueID = ctxt.TestCollection.TestAssembly.UniqueID;
		var testCollectionUniqueID = ctxt.TestCollection.UniqueID;

		var collectionStarting = new _TestCollectionStarting
		{
			AssemblyUniqueID = testAssemblyUniqueID,
			TestCollectionClass = ctxt.TestCollection.CollectionDefinition?.Name,
			TestCollectionDisplayName = ctxt.TestCollection.DisplayName,
			TestCollectionUniqueID = testCollectionUniqueID
		};

		if (!ctxt.MessageBus.QueueMessage(collectionStarting))
			ctxt.CancellationTokenSource.Cancel();
		else
		{
			try
			{
				await AfterTestCollectionStartingAsync(ctxt);

				SetTestContext(ctxt, TestEngineStatus.Running);

				collectionSummary = await RunTestClassesAsync(ctxt);

				SetTestContext(ctxt, TestEngineStatus.CleaningUp);

				ctxt.Aggregator.Clear();
				await BeforeTestCollectionFinishedAsync(ctxt);

				if (ctxt.Aggregator.HasExceptions)
				{
					var collectionCleanupFailure = _TestCollectionCleanupFailure.FromException(ctxt.Aggregator.ToException()!, testAssemblyUniqueID, testCollectionUniqueID);
					if (!ctxt.MessageBus.QueueMessage(collectionCleanupFailure))
						ctxt.CancellationTokenSource.Cancel();
				}
			}
			finally
			{
				var collectionFinished = new _TestCollectionFinished
				{
					AssemblyUniqueID = testAssemblyUniqueID,
					ExecutionTime = collectionSummary.Time,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestsFailed = collectionSummary.Failed,
					TestsNotRun = collectionSummary.NotRun,
					TestsTotal = collectionSummary.Total,
					TestsSkipped = collectionSummary.Skipped
				};

				if (!ctxt.MessageBus.QueueMessage(collectionFinished))
					ctxt.CancellationTokenSource.Cancel();
			}
		}

		return collectionSummary;
	}

	/// <summary>
	/// Runs the list of test classes. By default, groups the tests by class and runs them synchronously.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected virtual async ValueTask<RunSummary> RunTestClassesAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var summary = new RunSummary();

		foreach (var testCasesByClass in ctxt.TestCases.GroupBy(tc => tc.TestClass, TestClassComparer.Instance))
		{
			if (testCasesByClass.Key is null)
				TestContext.Current?.SendDiagnosticMessage(
					"TestCollectionRunner was given a null type to run for test case(s): {0}",
					string.Join(", ", testCasesByClass.Select(tcc => string.Format(CultureInfo.CurrentCulture, "'{0}'", tcc.TestCaseDisplayName)))
				);
			else if (testCasesByClass.Key.Class is not _IReflectionTypeInfo reflectionTypeInfo)
				TestContext.Current?.SendDiagnosticMessage(
					"TestCollectionRunner was given a non-reflection-backed type to run ('{0}') for test case(s): {1}",
					testCasesByClass.Key.Class.Name,
					string.Join(", ", testCasesByClass.Select(tcc => string.Format(CultureInfo.CurrentCulture, "'{0}'", tcc.TestCaseDisplayName)))
				);
			else
				summary.Aggregate(await RunTestClassAsync(ctxt, testCasesByClass.Key, reflectionTypeInfo, testCasesByClass.CastOrToReadOnlyCollection()));

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
	/// <param name="class">The CLR class that contains the tests to be run. May be <c>null</c> for test
	/// cases that do not support classes and methods.</param>
	/// <param name="testCases">The test cases to be run.</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected abstract ValueTask<RunSummary> RunTestClassAsync(
		TContext ctxt,
		_ITestClass? testClass,
		_IReflectionTypeInfo? @class,
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
