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

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestCollections(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (ctxt.DisableParallelization || exception is not null)
			return await base.RunTestCollections(ctxt, exception);

		ctxt.SetupParallelism();

		Func<Func<ValueTask<RunSummary>>, ValueTask<RunSummary>> taskRunner;
		if (SynchronizationContext.Current is not null)
		{
			var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
			taskRunner = code => new(Task.Factory.StartNew(() => code().AsTask(), ctxt.CancellationTokenSource.Token, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.HideScheduler, scheduler).Unwrap());
		}
		else
			taskRunner = code => new(Task.Run(() => code().AsTask(), ctxt.CancellationTokenSource.Token));

		List<ValueTask<RunSummary>>? parallel = null;
		List<Func<ValueTask<RunSummary>>>? nonParallel = null;
		var summaries = new List<RunSummary>();

		foreach (var (collection, testCases) in OrderTestCollections(ctxt))
		{
			ValueTask<RunSummary> task() => RunTestCollection(ctxt, collection, testCases);
			if (collection.DisableParallelization)
				(nonParallel ??= []).Add(task);
			else
				(parallel ??= []).Add(taskRunner(task));
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
