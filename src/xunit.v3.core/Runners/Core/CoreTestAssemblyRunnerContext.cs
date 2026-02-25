using System.Security;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CoreTestAssemblyRunner{TContext, TTestAssembly, TTestCollection, TTestCase}"/>.
/// </summary>
/// <param name="testAssembly">The test assembly</param>
/// <param name="testCases">The test cases from the assembly</param>
/// <param name="executionMessageSink">The message sink to send execution messages to</param>
/// <param name="executionOptions">The options used during test execution</param>
/// <param name="cancellationToken">The cancellation token used to cancel execution</param>
/// <typeparam name="TTestAssembly">The type of the test assembly used by the test framework. Must
/// derive from <see cref="ICoreTestAssembly"/>.</typeparam>
/// <typeparam name="TTestCollection">The type of the test collection used by the test framework. Must
/// derive from <see cref="ICoreTestCase"/>.</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="ICoreTestCase"/>.</typeparam>
/// <remarks>
/// This class is shared between reflection-based and code generation-based tests.
/// </remarks>
public abstract class CoreTestAssemblyRunnerContext<TTestAssembly, TTestCollection, TTestCase>(
	TTestAssembly testAssembly,
	IReadOnlyCollection<TTestCase> testCases,
	IMessageSink executionMessageSink,
	ITestFrameworkExecutionOptions executionOptions,
	CancellationToken cancellationToken) :
		TestAssemblyRunnerContext<TTestAssembly, TTestCase>(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken)
			where TTestAssembly : class, ICoreTestAssembly
			where TTestCollection : class, ICoreTestCollection
			where TTestCase : class, ICoreTestCase
{
	SemaphoreSlim? parallelSemaphore;
	MaxConcurrencySyncContext? syncContext;

	/// <summary>
	/// Gets a flag which indicates whether the user has requested that parallelization be disabled.
	/// </summary>
	public virtual bool DisableParallelization =>
		ExecutionOptions.DisableParallelization() ?? TestAssembly.DisableParallelization ?? false;

	/// <summary>
	/// Gets a flag which indicates how explicit tests should be handled.
	/// </summary>
	public virtual ExplicitOption ExplicitOption =>
		ExecutionOptions.ExplicitOptionOrDefault();

	/// <summary>
	/// Gets the maximum number of threads to use when running tests in parallel. If this returns a
	/// positive integer, that is the maximum number of threads; if it returns -1, that indicates that
	/// unlimited threads should be allowed.
	/// </summary>
	public virtual int MaxParallelThreads =>
		ExecutionOptions.MaxParallelThreads() ?? TestAssembly.MaxParallelThreads switch
		{
			0 or null => Environment.ProcessorCount,
			int value => value,
		};

	/// <summary>
	/// Gets the algorithm used for parallelism.
	/// </summary>
	public virtual ParallelAlgorithm ParallelAlgorithm =>
		ExecutionOptions.ParallelAlgorithm() ?? TestAssembly.ParallelAlgorithm switch
		{
			ParallelAlgorithm.Aggressive => ParallelAlgorithm.Aggressive,
			_ => ParallelAlgorithm.Conservative,  // implicit invalid value validation/conversion to default
		};

	/// <inheritdoc/>
	public override string TargetFramework =>
		TestAssembly.TargetFramework;

	/// <inheritdoc/>
	public override string TestEnvironment
	{
		get
		{
			var maxParallelThreads = MaxParallelThreads;
			var threadCountText = maxParallelThreads < 0 ? "unlimited" : maxParallelThreads.ToString(CultureInfo.CurrentCulture);
			threadCountText += " thread";
			if (maxParallelThreads != 1)
				threadCountText += 's';
			if (maxParallelThreads > 0 && ParallelAlgorithm == ParallelAlgorithm.Aggressive)
				threadCountText += "/aggressive";

			return string.Format(
				CultureInfo.CurrentCulture,
				"{0} [{1}, {2}]",
				base.TestEnvironment,
				GetTestCollectionFactoryDisplayName(),
				DisableParallelization
					? "non-parallel"
					: string.Format(CultureInfo.CurrentCulture, "parallel ({0})", threadCountText)
			);
		}
	}

	/// <summary>
	/// To be called after the test collection has been executed.
	/// </summary>
	public void AfterTestCollection() =>
		parallelSemaphore?.Release();

	/// <summary>
	/// To be called before executing a test collection.
	/// </summary>
	public async ValueTask BeforeTestCollection()
	{
		if (parallelSemaphore is not null)
			await parallelSemaphore.WaitAsync(TestContext.Current.CancellationToken);
	}

	/// <inheritdoc/>
	public override async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		if (syncContext is IAsyncDisposable asyncDisposable)
			await asyncDisposable.SafeDisposeAsync();
		else if (syncContext is IDisposable disposable)
			disposable.SafeDispose();

		parallelSemaphore?.Dispose();

		await base.DisposeAsync();
	}

	/// <summary>
	/// Gets the test collection factory display name, to be used in <see cref="TestEnvironment"/>.
	/// </summary>
	protected abstract string GetTestCollectionFactoryDisplayName();

	/// <summary>
	/// Runs the test collection.
	/// </summary>
	/// <param name="testCollection">The test collection to run</param>
	/// <param name="testCases">The test cases in the test collection</param>
	/// <remarks>
	/// The orderers provided here come from the test assembly.
	/// </remarks>
	public abstract ValueTask<RunSummary> RunTestCollection(
		TTestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases);

	/// <summary>
	/// Sets up the mechanics for parallelism.
	/// </summary>
	public virtual void SetupParallelism()
	{
		var maxParallelThreads = MaxParallelThreads;

		// When unlimited, we just launch everything and let the .NET Thread Pool sort it out
		if (maxParallelThreads < 0)
			return;

		// For aggressive, we launch everything and let our sync context limit what's allowed to run
		if (ParallelAlgorithm == ParallelAlgorithm.Aggressive)
		{
			syncContext = new MaxConcurrencySyncContext(maxParallelThreads);
			SetupSyncContextInternal(syncContext);
		}
		// For conversative, we use a semaphore to limit the number of launched tests, and ensure
		// that the .NET Thread Pool has enough threads based on the user's requested maximum
		else
		{
			parallelSemaphore = new(initialCount: maxParallelThreads);

			ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);
			var threadFloor = Math.Min(4, maxParallelThreads);
			if (workerThreads < threadFloor)
				ThreadPool.SetMinThreads(threadFloor, completionPortThreads);
		}
	}

	[SecuritySafeCritical]
	static void SetupSyncContextInternal(SynchronizationContext? context) =>
		SynchronizationContext.SetSynchronizationContext(context);
}
