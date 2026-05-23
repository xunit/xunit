using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CodeGenTestAssemblyRunnerBase{TContext, TTestAssembly, TTestCollection, TTestCase}"/>.
/// </summary>
/// <param name="testAssembly">The test assembly</param>
/// <param name="testCases">The test cases from the assembly</param>
/// <param name="executionMessageSink">The message sink to send execution messages to</param>
/// <param name="executionOptions">The options used during test execution</param>
/// <param name="cancellationToken">The cancellation token used to cancel execution</param>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public abstract class CodeGenTestAssemblyRunnerBaseContext<TTestAssembly, TTestCollection, TTestCase>(
	TTestAssembly testAssembly,
	IReadOnlyCollection<TTestCase> testCases,
	IMessageSink executionMessageSink,
	ITestFrameworkExecutionOptions executionOptions,
	CancellationToken cancellationToken) :
		CoreTestAssemblyRunnerContext<TTestAssembly, TTestCollection, TTestCase>(testAssembly, testCases, executionMessageSink, executionOptions, cancellationToken)
			where TTestAssembly : class, ICodeGenTestAssembly
			where TTestCollection : class, ICodeGenTestCollection
			where TTestCase : class, ICodeGenTestCase
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
}
