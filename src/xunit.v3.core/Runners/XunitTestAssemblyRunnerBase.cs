using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test assembly runner for xUnit.net v3 tests (with overridable context).
/// </summary>
public class XunitTestAssemblyRunnerBase<TContext, TTestAssembly, TTestCollection, TTestCase> :
	TestAssemblyRunner<TContext, TTestAssembly, TTestCollection, TTestCase>
		where TContext : XunitTestAssemblyRunnerBaseContext<TTestAssembly, TTestCase>
		where TTestAssembly : class, IXunitTestAssembly
		where TTestCollection : class, IXunitTestCollection
		where TTestCase : class, IXunitTestCase
{
	/// <inheritdoc/>
	protected override ValueTask<string> GetTestFrameworkDisplayName(TContext ctxt) =>
		new(XunitTestFramework.DisplayName);

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestAssemblyFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.AssemblyFixtureMappings.DisposeAsync);
		return await base.OnTestAssemblyFinished(ctxt, summary);
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestAssemblyStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = await base.OnTestAssemblyStarting(ctxt);
		await ctxt.Aggregator.RunAsync(() => ctxt.AssemblyFixtureMappings.InitializeAsync(ctxt.TestAssembly.AssemblyFixtureTypes));
		return result;
	}

	/// <inheritdoc/>
	protected override List<(TTestCollection Collection, List<TTestCase> TestCases)> OrderTestCollections(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var testCollectionOrderer = ctxt.AssemblyTestCollectionOrderer ?? DefaultTestCollectionOrderer.Instance;
		var testCasesByCollection =
			ctxt.TestCases
				.GroupBy(tc => (TTestCollection)tc.TestCollection, TestCollectionComparer<TTestCollection>.Instance)
				.ToDictionary(collectionGroup => collectionGroup.Key, collectionGroup => collectionGroup.ToList());

		IReadOnlyCollection<TTestCollection> orderedTestCollections;

		try
		{
			orderedTestCollections = testCollectionOrderer.OrderTestCollections(testCasesByCollection.Keys);
		}
		catch (Exception ex)
		{
			var innerEx = ex.Unwrap();

			ctxt.MessageBus.QueueMessage(new ErrorMessage()
			{
				ExceptionParentIndices = [-1],
				ExceptionTypes = [typeof(TestPipelineException).SafeName()],
				Messages = [
					string.Format(
						CultureInfo.CurrentCulture,
						"Assembly-level test collection orderer '{0}' threw '{1}' during ordering: {2}",
						testCollectionOrderer.GetType().SafeName(),
						innerEx.GetType().SafeName(),
						innerEx.Message
					)
				],
				StackTraces = [innerEx.StackTrace],
			});

			return [];
		}

		return
			orderedTestCollections
				.Select(collection => (collection, testCasesByCollection[collection]))
				.ToList();
	}

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

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestCollection(
		TContext ctxt,
		TTestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(testCollection);
		Guard.ArgumentNotNull(testCases);

		var testCaseOrderer = ctxt.AssemblyTestCaseOrderer ?? DefaultTestCaseOrderer.Instance;

		return ctxt.RunTestCollection(testCollection, testCases, testCaseOrderer);
	}
}
