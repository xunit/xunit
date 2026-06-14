using System.ComponentModel;
using System.Runtime.CompilerServices;
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
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestAssemblyRunnerBaseContext<TTestAssembly, TTestCollection, TTestCase>(
	TTestAssembly testAssembly,
	IReadOnlyCollection<TTestCase> testCases,
	IMessageSink executionMessageSink,
	ITestFrameworkExecutionOptions executionOptions,
	CancellationToken cancellationToken) :
		CoreTestAssemblyRunnerContext<TTestAssembly, TTestCollection, TTestCase>(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken)
			where TTestAssembly : class, IXunitTestAssembly
			where TTestCollection : class, IXunitTestCollection
			where TTestCase : class, IXunitTestCase
{
	/// <summary>
	/// Gets the mapping manager for assembly-level fixtures.
	/// </summary>
	public FixtureMappingManager AssemblyFixtureMappings { get; } = new("Assembly");

	/// <summary>
	/// Please read <see cref="CoreTestAssemblyRunnerContext{TTestAssembly, TTestCollection, TTestCase}.ParallelMode"/> instead.
	/// This property will be removed in the next major version.
	/// </summary>
	[Obsolete("Please read ParallelMode instead. This property will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool DisableParallelization =>
		ParallelMode == ParallelMode.None;

	/// <inheritdoc/>
	protected override string GetTestCollectionFactoryDisplayName() =>
		RegisteredEngineConfig.GetTestCollectionFactory(TestAssembly).DisplayName;

	/// <summary>
	/// Please use <see cref="RunTestCollection(TTestCollection, IReadOnlyCollection{TTestCase})"/>.
	/// This overload will be removed in the next major version.
	/// </summary>
	[Obsolete("Please use the overload which does not include testCaseOrderer. This overload will be removed in the next major version.")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[OverloadResolutionPriority(-1)]
	public ValueTask<RunSummary> RunTestCollection(
		TTestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases,
		ITestCaseOrderer testCaseOrderer) =>
			RunTestCollection(testCollection, testCases);

	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunTestCollection(
		TTestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases) =>
			XunitTestCollectionRunner.Instance.Run(
				testCollection,
				testCases,
				ExplicitOption,
				MessageBus,
				Aggregator.Clone(),
				CancellationTokenSource,
				ParallelMode,
				Scheduler,
				AssemblyFixtureMappings
			);

	/// <summary>
	/// This method has been replaced by <see cref="CoreTestAssemblyRunnerContext{TTestAssembly, TTestCollection, TTestCase}.CreateScheduler"/>,
	/// and is no longer called. This method will be removed in the next major version.
	/// </summary>
	[Obsolete("This method has been replaced by CreateScheduler in the context, and is no longer called. This method will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual void SetupParallelism() =>
		throw new NotSupportedException("This method has been replaced by CreateScheduler, and is no longer called. This method will be removed in the next major version.");
}
