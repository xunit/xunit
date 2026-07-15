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
		CodeGenTestAssemblyRunnerBaseContext<ICodeGenTestAssembly, ICodeGenTestCollection, ICodeGenTestCase>(
			testAssembly,
			testCases,
			executionMessageSink,
			executionOptions,
			cancellationToken
		)
{
	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunTestCollection(
		ICodeGenTestCollection testCollection,
		IReadOnlyCollection<ICodeGenTestCase> testCases) =>
			CodeGenTestCollectionRunner.Instance.Run(
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
}
