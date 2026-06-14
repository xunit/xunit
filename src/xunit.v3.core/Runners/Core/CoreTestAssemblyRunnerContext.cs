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
	ExecutionScheduler? executionScheduler;

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

	/// <summary>
	/// Gets the default parallelization mode for the test assembly.
	/// </summary>
	public virtual ParallelMode ParallelMode =>
		ExecutionOptions.ParallelMode() ?? TestAssembly.ParallelMode switch
		{
			ParallelMode.None => ParallelMode.None,
			ParallelMode.All => ParallelMode.All,
			_ => ParallelMode.Collections,  // implicit invalid value validation/conversion to default
		};

	/// <summary>
	/// Gets the execution scheduler.
	/// </summary>
	public virtual ExecutionScheduler Scheduler
	{
		get
		{
			executionScheduler ??= CreateScheduler();
			return executionScheduler;
		}
	}

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
				ParallelMode switch
				{
					ParallelMode.All => string.Format(CultureInfo.CurrentCulture, "parallel (all, {0})", threadCountText),
					ParallelMode.Collections => string.Format(CultureInfo.CurrentCulture, "parallel (collections, {0})", threadCountText),
					_ => "non-parallel",
				}
			);
		}
	}

	/// <summary>
	/// Creates the execution scheduler.
	/// </summary>
	protected virtual ExecutionScheduler CreateScheduler() =>
		ExecutionScheduler.Create(MaxParallelThreads, ParallelAlgorithm);

	/// <inheritdoc/>
	public override async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		if (executionScheduler is not null)
			await executionScheduler.SafeDisposeAsync();

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
}
