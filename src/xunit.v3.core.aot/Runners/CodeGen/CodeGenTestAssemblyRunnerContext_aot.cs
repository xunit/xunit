using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CodeGenTestAssemblyRunner"/>.
/// </summary>
/// <param name="testAssembly">The test assembly</param>
/// <param name="testCases">The test cases from the assembly</param>
/// <param name="executionMessageSink">The message sink to send execution messages to</param>
/// <param name="executionOptions">The options used during test execution</param>
/// <param name="cancellationToken">The cancellation token used to cancel execution</param>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestAssemblyRunnerContext(
	ICodeGenTestAssembly testAssembly,
	IReadOnlyCollection<ICodeGenTestCase> testCases,
	IMessageSink executionMessageSink,
	ITestFrameworkExecutionOptions executionOptions,
	CancellationToken cancellationToken) :
		CoreTestAssemblyRunnerContext<ICodeGenTestAssembly, ICodeGenTestCollection, ICodeGenTestCase>(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken)
{
	/// <summary>
	/// Gets the mapping manager for assembly-level fixtures.
	/// </summary>
	public FixtureMappingManager AssemblyFixtureMappings { get; } = new("Assembly", Guard.ArgumentNotNull(testAssembly).AssemblyFixtureFactories);

	/// <inheritdoc/>
	public override async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		await AssemblyFixtureMappings.SafeDisposeAsync();
		await base.DisposeAsync();
	}

	/// <inheritdoc/>
	protected override string GetTestCollectionFactoryDisplayName() =>
		TestAssembly.TestCollectionFactory.DisplayName;

	/// <inheritdoc/>
	public override async ValueTask<RunSummary> RunTestCollection(
		ICodeGenTestCollection testCollection,
		IReadOnlyCollection<ICodeGenTestCase> testCases)
	{
		await BeforeTestCollection();

		try
		{
			return await CodeGenTestCollectionRunner.Instance.Run(
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
