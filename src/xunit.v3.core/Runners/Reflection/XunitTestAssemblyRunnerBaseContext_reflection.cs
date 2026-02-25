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

	/// <inheritdoc/>
	protected override string GetTestCollectionFactoryDisplayName() =>
		RegisteredEngineConfig.GetTestCollectionFactory(TestAssembly).DisplayName;

	/// <summary>
	/// Please use <see cref="RunTestCollection(TTestCollection, IReadOnlyCollection{TTestCase})"/>.
	/// This overload will be removed in the next major version.
	/// </summary>
	[Obsolete("Please use the overload which does not include testCaseOrderer. This overload will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[OverloadResolutionPriority(-1)]
	public ValueTask<RunSummary> RunTestCollection(
		TTestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases,
		ITestCaseOrderer testCaseOrderer) =>
			RunTestCollection(testCollection, testCases);

	/// <inheritdoc/>
	public override async ValueTask<RunSummary> RunTestCollection(
		TTestCollection testCollection,
		IReadOnlyCollection<TTestCase> testCases)
	{
		await BeforeTestCollection();

		try
		{
			return await XunitTestCollectionRunner.Instance.Run(
				testCollection,
				testCases,
				ExplicitOption,
				MessageBus,
				Aggregator.Clone(),
				CancellationTokenSource,
				AssemblyFixtureMappings
			);
		}
		finally
		{
			AfterTestCollection();
		}
	}
}
