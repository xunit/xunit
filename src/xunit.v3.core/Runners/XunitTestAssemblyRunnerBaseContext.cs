using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestAssemblyRunnerBase{TContext, TTestAssembly, TTestCollection, TTestCase}"/>.
/// </summary>
/// <param name="testAssembly">The test assembly</param>
/// <param name="testCases">The test cases from the assembly</param>
/// <param name="executionMessageSink">The message sink to send execution messages to</param>
/// <param name="executionOptions">The options used during test execution</param>
/// <param name="cancellationToken">The cancellation token used to cancel execution</param>
public class XunitTestAssemblyRunnerBaseContext<TTestAssembly, TTestCase>(
	TTestAssembly testAssembly,
	IReadOnlyCollection<TTestCase> testCases,
	IMessageSink executionMessageSink,
	ITestFrameworkExecutionOptions executionOptions,
	CancellationToken cancellationToken) :
		TestAssemblyRunnerContext<TTestAssembly, TTestCase>(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken)
			where TTestAssembly : class, IXunitTestAssembly
			where TTestCase : class, IXunitTestCase
{
	ICollectionBehaviorAttribute? collectionBehaviorAttribute;
	SemaphoreSlim? parallelSemaphore;
	MaxConcurrencySyncContext? syncContext;

	/// <summary>
	/// Gets the mapping manager for assembly-level fixtures.
	/// </summary>
	public FixtureMappingManager AssemblyFixtureMappings { get; } = new("Assembly");

	/// <summary>
	/// Gets the assembly-level test case orderer, if one is present.
	/// </summary>
	public ITestCaseOrderer? AssemblyTestCaseOrderer { get; private set; }

	/// <summary>
	/// Gets the assembly-level test collection orderer, if one is present.
	/// </summary>
	public ITestCollectionOrderer? AssemblyTestCollectionOrderer { get; private set; }

	/// <summary>
	/// Gets a flag which indicates whether the user has requested that parallelization be disabled.
	/// </summary>
	public bool DisableParallelization { get; private set; }

	/// <summary>
	/// Gets a flag which indicates how explicit tests should be handled.
	/// </summary>
	public ExplicitOption ExplicitOption => ExecutionOptions.ExplicitOptionOrDefault();

	/// <summary>
	/// Gets the maximum number of threads to use when running tests in parallel. If this returns a
	/// positive integer, that is the maximum number of threads; if it returns -1, that indicates that
	/// unlimited threads should be allowed.
	/// </summary>
	public int MaxParallelThreads { get; private set; }

	/// <summary>
	/// Gets the algorithm used for parallelism.
	/// </summary>
	public ParallelAlgorithm ParallelAlgorithm { get; private set; }

	/// <summary>
	/// Gets the assembly that is being executed.
	/// </summary>
	public new TTestAssembly TestAssembly { get; } = Guard.ArgumentNotNull(testAssembly);

	/// <inheritdoc/>
	public override string TargetFramework =>
		TestAssembly.TargetFramework;

	/// <inheritdoc/>
	public override string TestEnvironment
	{
		get
		{
			var testCollectionFactory =
				ExtensibilityPointFactory.GetXunitTestCollectionFactory(collectionBehaviorAttribute?.CollectionFactoryType, TestAssembly)
					?? new CollectionPerClassTestCollectionFactory(TestAssembly);

			var threadCountText = MaxParallelThreads < 0 ? "unlimited" : MaxParallelThreads.ToString(CultureInfo.CurrentCulture);
			threadCountText += " thread";
			if (MaxParallelThreads != 1)
				threadCountText += 's';
			if (MaxParallelThreads > 0 && ParallelAlgorithm == ParallelAlgorithm.Aggressive)
				threadCountText += "/aggressive";

			return string.Format(
				CultureInfo.CurrentCulture,
				"{0} [{1}, {2}]",
				base.TestEnvironment,
				testCollectionFactory.DisplayName,
				DisableParallelization
					? "non-parallel"
					: string.Format(CultureInfo.CurrentCulture, "parallel ({0})", threadCountText)
			);
		}
	}

	/// <inheritdoc/>
	public override async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		if (syncContext is IAsyncDisposable asyncDisposable)
			await asyncDisposable.DisposeAsync();
		else if (syncContext is IDisposable disposable)
			disposable.Dispose();

		parallelSemaphore?.Dispose();

		await base.DisposeAsync();
	}

	/// <inheritdoc/>
	public override async ValueTask InitializeAsync()
	{
		await base.InitializeAsync();

		Aggregator.Run(() =>
		{
			collectionBehaviorAttribute = TestAssembly.CollectionBehavior;
			if (collectionBehaviorAttribute is not null)
			{
				DisableParallelization = collectionBehaviorAttribute.DisableTestParallelization;
				MaxParallelThreads = collectionBehaviorAttribute.MaxParallelThreads;
				ParallelAlgorithm = Guard.ArgumentEnumValid(collectionBehaviorAttribute.ParallelAlgorithm, [ParallelAlgorithm.Aggressive, ParallelAlgorithm.Conservative]);
			}

			DisableParallelization = ExecutionOptions.DisableParallelization() ?? DisableParallelization;
			MaxParallelThreads = ExecutionOptions.MaxParallelThreads() ?? MaxParallelThreads;
			ParallelAlgorithm = ExecutionOptions.ParallelAlgorithm() ?? ParallelAlgorithm;

			if (MaxParallelThreads == 0)
				MaxParallelThreads = Environment.ProcessorCount;

			AssemblyTestCaseOrderer = TestAssembly.TestCaseOrderer;
			AssemblyTestCollectionOrderer = TestAssembly.TestCollectionOrderer;
		});
	}

	/// <summary>
	/// Delegation of <see cref="XunitTestAssemblyRunnerBase{TContext, TTestAssembly, TTestCollection, TTestCase}.RunTestCollection"/>
	/// that properly obeys the parallel algorithm requirements.
	/// </summary>
	public async ValueTask<RunSummary> RunTestCollection(
		IXunitTestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases,
		ITestCaseOrderer testCaseOrderer)
	{
		if (parallelSemaphore is not null)
			await parallelSemaphore.WaitAsync(CancellationTokenSource.Token);

		try
		{
			return await XunitTestCollectionRunner.Instance.Run(
				testCollection,
				testCases,
				ExplicitOption,
				MessageBus,
				testCaseOrderer,
				Aggregator.Clone(),
				CancellationTokenSource,
				AssemblyFixtureMappings
			);
		}
		finally
		{
			parallelSemaphore?.Release();
		}
	}

	/// <summary>
	/// Sets up the mechanics for parallelism.
	/// </summary>
	public virtual void SetupParallelism()
	{
		// When unlimited, we just launch everything and let the .NET Thread Pool sort it out
		if (MaxParallelThreads < 0)
			return;

		// For aggressive, we launch everything and let our sync context limit what's allowed to run
		if (ParallelAlgorithm == ParallelAlgorithm.Aggressive)
		{
			syncContext = new MaxConcurrencySyncContext(MaxParallelThreads);
			SetupSyncContextInternal(syncContext);
		}
		// For conversative, we use a semaphore to limit the number of launched tests, and ensure
		// that the .NET Thread Pool has enough threads based on the user's requested maximum
		else
		{
			parallelSemaphore = new(initialCount: MaxParallelThreads);

			ThreadPool.GetMinThreads(out var minThreads, out var minIOPorts);
			var threadFloor = Math.Min(4, MaxParallelThreads);
			if (minThreads < threadFloor)
				ThreadPool.SetMinThreads(threadFloor, minIOPorts);
		}
	}

	[SecuritySafeCritical]
	static void SetupSyncContextInternal(SynchronizationContext? context) =>
		SynchronizationContext.SetSynchronizationContext(context);
}
