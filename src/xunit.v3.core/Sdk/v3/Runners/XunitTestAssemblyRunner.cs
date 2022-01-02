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
		await CreateAssemblyFixturesAsync(ctxt);
		await base.AfterTestAssemblyStartingAsync(ctxt);
	}

	/// <inheritdoc/>
	protected override async ValueTask BeforeTestAssemblyFinishedAsync(XunitTestAssemblyRunnerContext ctxt)
	{
		var disposeAsyncTasks =
			ctxt.AssemblyFixtureMappings
				.Values
				.OfType<IAsyncDisposable>()
				.Select(fixture => ctxt.Aggregator.RunAsync(async () =>
				{
					try
					{
						await fixture.DisposeAsync();
					}
					catch (Exception ex)
					{
						throw new TestFixtureCleanupException($"Assembly fixture type '{fixture.GetType().FullName}' threw in DisposeAsync", ex.Unwrap());
					}
				}).AsTask())
				.ToList();

		await Task.WhenAll(disposeAsyncTasks);

		foreach (var fixture in ctxt.AssemblyFixtureMappings.Values.OfType<IDisposable>())
			ctxt.Aggregator.Run(() =>
			{
				try
				{
					fixture.Dispose();
				}
				catch (Exception ex)
				{
					throw new TestFixtureCleanupException($"Assembly fixture type '{fixture.GetType().FullName}' threw in Dispose", ex.Unwrap());
				}
			});

		await base.BeforeTestAssemblyFinishedAsync(ctxt);
	}

	/// <summary>
	/// Creates the instance of a assembly fixture type to be used by the test assembly. If the fixture can be created,
	/// it should be placed into the AssemblyFixtureMappings dictionary in <paramref name="ctxt"/>; if it cannot, then
	/// the method should record the error by calling <code>Aggregator.Add</code>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test assembly</param>
	/// <param name="fixtureType">The type of the fixture to be created</param>
	protected virtual void CreateAssemblyFixture(
		XunitTestAssemblyRunnerContext ctxt,
		Type fixtureType)
	{
		var ctors =
			fixtureType
				.GetConstructors()
				.Where(ci => !ci.IsStatic && ci.IsPublic)
				.ToList();

		if (ctors.Count != 1)
		{
			ctxt.Aggregator.Add(new TestClassException($"Assembly fixture type '{fixtureType.FullName}' may only define a single public constructor."));
			return;
		}

		var ctor = ctors[0];
		var missingParameters = new List<ParameterInfo>();
		var ctorArgs = ctor.GetParameters().Select(p =>
		{
			object? arg = null;
			if (p.ParameterType == typeof(_IMessageSink))
				arg = TestContext.Current?.DiagnosticMessageSink;
			else if (p.ParameterType == typeof(ITestContextAccessor))
				arg = TestContextAccessor.Instance;
			else
				missingParameters.Add(p);
			return arg;
		}).ToArray();

		if (missingParameters.Count > 0)
			ctxt.Aggregator.Add(new TestClassException(
				$"Assembly fixture type '{fixtureType.FullName}' had one or more unresolved constructor arguments: {string.Join(", ", missingParameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))}"
			));
		else
			ctxt.Aggregator.Run(() =>
			{
				try
				{
					ctxt.AssemblyFixtureMappings[fixtureType] = ctor.Invoke(ctorArgs);
				}
				catch (Exception ex)
				{
					throw new TestClassException($"Assembly fixture type '{fixtureType.FullName}' threw in its constructor", ex.Unwrap());
				}
			});
	}

	ValueTask CreateAssemblyFixturesAsync(XunitTestAssemblyRunnerContext ctxt)
	{
		foreach (var attributeInfo in ctxt.TestAssembly.Assembly.GetCustomAttributes(typeof(AssemblyFixtureAttribute)))
		{
			var fixtureType = attributeInfo.GetConstructorArguments().Single() as Type;
			if (fixtureType != null)
				CreateAssemblyFixture(ctxt, fixtureType);
		}

		var initializeAsyncTasks =
			ctxt.AssemblyFixtureMappings
				.Values
				.OfType<IAsyncLifetime>()
				.Select(
					fixture => ctxt.Aggregator.RunAsync(async () =>
					{
						try
						{
							await fixture.InitializeAsync();
						}
						catch (Exception ex)
						{
							throw new TestClassException($"Assembly fixture type '{fixture.GetType().FullName}' threw in InitializeAsync", ex.Unwrap());
						}
					}).AsTask()
				)
				.ToList();

		return new(Task.WhenAll(initializeAsyncTasks));
	}

	/// <inheritdoc/>
	protected override ITestCaseOrderer GetTestCaseOrderer(XunitTestAssemblyRunnerContext ctxt) =>
		ctxt.AssemblyTestCaseOrderer ?? base.GetTestCaseOrderer(ctxt);

	/// <inheritdoc/>
	protected override ITestCollectionOrderer GetTestCollectionOrderer(XunitTestAssemblyRunnerContext ctxt) =>
		ctxt.AssemblyTestCollectionOrderer ?? base.GetTestCollectionOrderer(ctxt);

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

		// Note: This method always needs to be async, so that it will ensure we restore any sync
		// context that might be set further downstream.
		return await RunAsync(new(testAssembly, testCases, executionMessageSink, executionOptions));
	}

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestCollectionsAsync(XunitTestAssemblyRunnerContext ctxt)
	{
		if (ctxt.DisableParallelization)
			return await base.RunTestCollectionsAsync(ctxt);

		ctxt.SetupMaxConcurrencySyncContext();

		Func<Func<ValueTask<RunSummary>>, ValueTask<RunSummary>> taskRunner;
		if (SynchronizationContext.Current != null)
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
			Skipped = summaries.Sum(s => s.Skipped)
		};
	}

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestCollectionAsync(
		XunitTestAssemblyRunnerContext ctxt,
		_ITestCollection testCollection,
		IReadOnlyCollection<IXunitTestCase> testCases) =>
			XunitTestCollectionRunner.Instance.RunAsync(
				testCollection,
				testCases,
				ctxt.MessageBus,
				GetTestCaseOrderer(ctxt),
				ctxt.Aggregator.Clone(),
				ctxt.CancellationTokenSource,
				ctxt.AssemblyFixtureMappings
			);
}
