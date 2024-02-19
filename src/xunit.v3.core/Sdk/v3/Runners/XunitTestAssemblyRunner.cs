using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test assembly runner for xUnit.net v3 tests.
/// </summary>
public class XunitTestAssemblyRunner : TestAssemblyRunner<XunitTestAssemblyRunnerContext, IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestAssemblyRunner"/> class.
	/// </summary>
	protected XunitTestAssemblyRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of <see cref="XunitTestAssemblyRunner"/>.
	/// </summary>
	public static XunitTestAssemblyRunner Instance { get; } = new();

	/// <inheritdoc/>
	protected override async ValueTask AfterTestAssemblyStartingAsync(XunitTestAssemblyRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var assemblyFixtureTypes =
			ctxt
				.TestAssembly
				.Assembly
				.GetCustomAttributes(typeof(AssemblyFixtureAttribute))
				.Select(a => a.GetConstructorArguments().Single() as Type)
				.WhereNotNull()
				.ToArray();

		await ctxt.Aggregator.RunAsync(() => ctxt.AssemblyFixtureMappings.InitializeAsync(assemblyFixtureTypes));

		await base.AfterTestAssemblyStartingAsync(ctxt);
	}

	/// <inheritdoc/>
	protected override async ValueTask BeforeTestAssemblyFinishedAsync(XunitTestAssemblyRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.AssemblyFixtureMappings.DisposeAsync);

		await base.BeforeTestAssemblyFinishedAsync(ctxt);
	}


	/// <inheritdoc/>
	protected override ITestCaseOrderer GetTestCaseOrderer(XunitTestAssemblyRunnerContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).AssemblyTestCaseOrderer ?? base.GetTestCaseOrderer(ctxt);

	/// <inheritdoc/>
	protected override ITestCollectionOrderer GetTestCollectionOrderer(XunitTestAssemblyRunnerContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).AssemblyTestCollectionOrderer ?? base.GetTestCollectionOrderer(ctxt);

	/// <summary>
	/// Runs the test assembly.
	/// </summary>
	/// <param name="testAssembly">The test assembly to be executed.</param>
	/// <param name="testCases">The test cases associated with the test assembly.</param>
	/// <param name="executionMessageSink">The message sink to send execution messages to.</param>
	/// <param name="executionOptions">The execution options to use when running tests.</param>
	public async ValueTask<RunSummary> RunAsync(
		_ITestAssembly testAssembly,
		IReadOnlyCollection<IXunitTestCase> testCases,
		_IMessageSink executionMessageSink,
		_ITestFrameworkExecutionOptions executionOptions)
	{
		Guard.ArgumentNotNull(testAssembly);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(executionMessageSink);
		Guard.ArgumentNotNull(executionOptions);

		await using var ctxt = new XunitTestAssemblyRunnerContext(testAssembly, testCases, executionMessageSink, executionOptions);
		await ctxt.InitializeAsync();

		return await RunAsync(ctxt);
	}

#pragma warning disable CA2012 // We guarantee that parallel ValueTasks are only awaited once

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestCollectionsAsync(XunitTestAssemblyRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		if (ctxt.DisableParallelization)
			return await base.RunTestCollectionsAsync(ctxt);

		ctxt.SetupMaxConcurrencySyncContext();

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

		foreach (var collection in OrderTestCollections(ctxt))
		{
			ValueTask<RunSummary> task() => RunTestCollectionAsync(ctxt, collection.Item1, collection.Item2);

			var attr = collection.Item1.CollectionDefinition?.GetCustomAttributes(typeof(CollectionDefinitionAttribute)).SingleOrDefault();
			if (attr?.GetNamedArgument<bool>(nameof(CollectionDefinitionAttribute.DisableParallelization)) == true)
				(nonParallel ??= new List<Func<ValueTask<RunSummary>>>()).Add(task);
			else
				(parallel ??= new List<ValueTask<RunSummary>>()).Add(taskRunner(task));
		}

		if (parallel?.Count > 0)
			foreach (var task in parallel)
				try
				{
					summaries.Add(await task);
				}
				catch (TaskCanceledException) { }

		if (nonParallel?.Count > 0)
			foreach (var task in nonParallel)
				try
				{
					summaries.Add(await taskRunner(task));
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
	protected override ValueTask<RunSummary> RunTestCollectionAsync(
		XunitTestAssemblyRunnerContext ctxt,
		_ITestCollection testCollection,
		IReadOnlyCollection<IXunitTestCase> testCases)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(testCollection);
		Guard.ArgumentNotNull(testCases);

		return XunitTestCollectionRunner.Instance.RunAsync(
			testCollection,
			testCases,
			ctxt.ExplicitOption,
			ctxt.MessageBus,
			GetTestCaseOrderer(ctxt),
			ctxt.Aggregator.Clone(),
			ctxt.CancellationTokenSource,
			ctxt.AssemblyFixtureMappings
		);
	}
}
