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

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestCollections(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var parallelMode = ctxt.ParallelMode;
		if (exception is not null || parallelMode == ParallelMode.None)
			return await base.RunTestCollections(ctxt, exception);

		List<ValueTask<RunSummary>>? parallelTasks = null;
		List<Func<ValueTask<RunSummary>>>? nonParallelTaskFactories = null;
		var summary = new RunSummary();

		foreach (var (testCollection, testCases) in OrderTestCollections(ctxt))
		{
			ValueTask<RunSummary> taskFactory() => RunTestCollection(ctxt, testCollection, testCases);

			if (testCollection.DisableParallelization)
				(nonParallelTaskFactories ??= []).Add(taskFactory);
			else
#pragma warning disable CA2012
				(parallelTasks ??= []).Add(parallelMode == ParallelMode.All ? taskFactory() : ctxt.Scheduler.RunParallelTask(taskFactory, ctxt.CancellationTokenSource.Token));
#pragma warning restore CA2012

			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				break;
		}

		if (parallelTasks?.Count > 0)
			foreach (var parallelTask in parallelTasks)
				try
				{
					summary.Aggregate(await parallelTask);
				}
				catch (TaskCanceledException) { }

		if (nonParallelTaskFactories?.Count > 0)
			foreach (var nonParallelTaskFactory in nonParallelTaskFactories)
				try
				{
					summary.Aggregate(await ctxt.Scheduler.RunSequentialTask(nonParallelTaskFactory, ctxt.CancellationTokenSource.Token));
				}
				catch (TaskCanceledException) { }

		return summary;
	}
}
