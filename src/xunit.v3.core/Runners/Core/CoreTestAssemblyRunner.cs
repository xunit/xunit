using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base test assembly runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is shared between reflection-based and code generation-based tests.
/// </remarks>
public abstract class CoreTestAssemblyRunner<TContext, TTestAssembly, TTestCollection, TTestCase> :
	TestAssemblyRunner<TContext, TTestAssembly, TTestCollection, TTestCase>
		where TContext : CoreTestAssemblyRunnerContext<TTestAssembly, TTestCollection, TTestCase>
		where TTestAssembly : class, ICoreTestAssembly
		where TTestCollection : class, ICoreTestCollection
		where TTestCase : class, ICoreTestCase
{
	/// <summary>
	/// Orders the test collections using the first available orderer from:
	/// <list type="bullet">
	/// <item><see cref="ICoreTestAssembly.TestCollectionOrderer"/></item>
	/// <item><see cref="DefaultTestCollectionOrderer"/></item>
	/// </list>
	/// </summary>
	/// <inheritdoc/>
	protected override List<(TTestCollection Collection, List<TTestCase> TestCases)> OrderTestCollections(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var testCasesByCollection =
			ctxt
				.TestCases
				.GroupBy(tc => (TTestCollection)tc.TestCollection, TestCollectionComparer<TTestCollection>.Instance)
				.ToDictionary(collectionGroup => collectionGroup.Key, collectionGroup => collectionGroup.ToList());

		var testCollectionOrderer =
			ctxt.TestAssembly.TestCollectionOrderer
				?? DefaultTestCollectionOrderer.Instance;

		try
		{
			var orderedTestCollections = testCollectionOrderer.OrderTestCollections(testCasesByCollection.Keys);

			return
				orderedTestCollections
					.Select(collection => (collection, testCasesByCollection[collection]))
					.ToList();
		}
		catch (Exception ex)
		{
			var innerEx = ex.Unwrap();

			ctxt.MessageBus.QueueMessage(new ErrorMessage()
			{
				AssemblyUniqueID = ctxt.TestAssembly.UniqueID,
				ExceptionParentIndices = [-1],
				ExceptionTypes = [typeof(TestPipelineException).SafeName()],
				Messages = [
					string.Format(
						CultureInfo.CurrentCulture,
						"Test collection orderer '{0}' threw '{1}' during ordering: {2}",
						testCollectionOrderer.GetType().SafeName(),
						innerEx.GetType().SafeName(),
						innerEx.Message ?? "(null message)"
					)
				],
				StackTraces = [innerEx.StackTrace],
			});

			return [];
		}
	}

	/// <summary>
	/// Runs the test collection via the context.
	/// </summary>
	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestCollection(
		TContext ctxt,
		TTestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases) =>
			Guard.ArgumentNotNull(ctxt).RunTestCollection(testCollection, testCases);

#pragma warning disable CA2012 // We guarantee that parallel ValueTasks are only awaited once

	/// <summary>
	/// Runs the list of test collections. Groups the tests by collection and runs them in parallel if
	/// <see cref="ParallelismOptions.Collections"/> is set, and serially otherwise.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Running"/> and any exceptions thrown will
	/// contribute to test assembly cleanup failure.
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <param name="exception">The exception that was caused during startup; should be used as an indicator that the
	/// downstream tests should fail with the provided exception rather than going through standard execution</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected override async ValueTask<RunSummary> RunTestCollections(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (!ctxt.ParallelismOptions.HasFlag(ParallelismOptions.Collections) || exception is not null)
			return await base.RunTestCollections(ctxt, exception);

		ctxt.SetupParallelism();

		var taskRunner = TestPipelineTaskRunner.Create(ctxt.CancellationTokenSource.Token);
		List<ValueTask<RunSummary>>? parallel = null;
		List<Func<ValueTask<RunSummary>>>? nonParallel = null;
		var summaries = new List<RunSummary>();

		foreach (var (collection, testCases) in OrderTestCollections(ctxt))
		{
			var semaphoreReleaser = ctxt.ParallelizationSemaphore != null
				? await ctxt.ParallelizationSemaphore.LockAsync(ctxt.CancellationTokenSource.Token)
				: null;

			try
			{
				if (collection.ParallelismOptions.HasFlag(ParallelismOptions.Collections))
					(parallel ??= []).Add(taskRunner(task));
				else
					(nonParallel ??= []).Add(task);
			}
			catch
			{
				semaphoreReleaser?.Dispose();
				throw;
			}

			async ValueTask<RunSummary> task()
			{
				using var _ = semaphoreReleaser;
				return await RunTestCollection(ctxt, collection, testCases);
			}
		}

		if (parallel?.Count > 0)
			foreach (var task in parallel)
				try
				{
					summaries.Add(await task);
				}
				catch (TaskCanceledException) { }

		if (nonParallel?.Count > 0)
			foreach (var taskFactory in nonParallel)
				try
				{
					summaries.Add(await taskRunner(taskFactory));
					if (ctxt.CancellationTokenSource.IsCancellationRequested)
						break;
				}
				catch (TaskCanceledException) { }

		return new RunSummary()
		{
			Total = summaries.Sum(s => s.Total),
			Failed = summaries.Sum(s => s.Failed),
			NotRun = summaries.Sum(s => s.NotRun),
			Skipped = summaries.Sum(s => s.Skipped),
		};
	}

#pragma warning restore CA2012
}
