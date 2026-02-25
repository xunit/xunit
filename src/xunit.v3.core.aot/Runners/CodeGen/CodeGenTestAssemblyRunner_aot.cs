using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test assembly runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestAssemblyRunner :
	CoreTestAssemblyRunner<CodeGenTestAssemblyRunnerContext, ICodeGenTestAssembly, ICodeGenTestCollection, ICodeGenTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CodeGenTestAssemblyRunner"/> class.
	/// </summary>
	protected CodeGenTestAssemblyRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of <see cref="CodeGenTestAssemblyRunner"/>.
	/// </summary>
	public static CodeGenTestAssemblyRunner Instance { get; } = new();

	/// <inheritdoc/>
	protected override ValueTask<string> GetTestFrameworkDisplayName(CodeGenTestAssemblyRunnerContext ctxt) =>
		new(CodeGenTestFramework.DisplayName);

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestAssemblyFinished(
		CodeGenTestAssemblyRunnerContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.AssemblyFixtureMappings.DisposeAsync);
		return await base.OnTestAssemblyFinished(ctxt, summary);
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestAssemblyStarting(CodeGenTestAssemblyRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = await base.OnTestAssemblyStarting(ctxt);
		await ctxt.Aggregator.RunAsync(() => ctxt.AssemblyFixtureMappings.InitializeAsync(
			createInstances: ctxt.TestCases.Any(tc => !tc.IsStaticallySkipped())
		));
		return result;
	}

	/// <summary>
	/// Runs the test assembly.
	/// </summary>
	/// <param name="testAssembly">The test assembly to be executed.</param>
	/// <param name="testCases">The test cases associated with the test assembly.</param>
	/// <param name="executionMessageSink">The message sink to send execution messages to.</param>
	/// <param name="executionOptions">The execution options to use when running tests.</param>
	/// <param name="cancellationToken">The cancellation token used to cancel test execution.</param>
	public async ValueTask<RunSummary> Run(
		ICodeGenTestAssembly testAssembly,
		IReadOnlyCollection<ICodeGenTestCase> testCases,
		IMessageSink executionMessageSink,
		ITestFrameworkExecutionOptions executionOptions,
		CancellationToken cancellationToken)
	{
		Guard.ArgumentNotNull(testAssembly);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(executionMessageSink);
		Guard.ArgumentNotNull(executionOptions);

		await using var ctxt = new CodeGenTestAssemblyRunnerContext(
			testAssembly,
			testCases,
			executionMessageSink,
			executionOptions,
			cancellationToken
		);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}
}
