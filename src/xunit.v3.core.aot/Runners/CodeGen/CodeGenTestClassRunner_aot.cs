using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test class runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestClassRunner : CodeGenTestClassRunnerBase<CodeGenTestClassRunnerContext, ICodeGenTestClass, ICodeGenTestMethod, ICodeGenTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CodeGenTestClassRunner"/> class.
	/// </summary>
	protected CodeGenTestClassRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of <see cref="CodeGenTestClassRunner"/>.
	/// </summary>
	public static CodeGenTestClassRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test class.
	/// </summary>
	/// <param name="testClass">The test class to be run.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="collectionFixtureMappings">The mapping of collection fixture types to fixtures.</param>
	/// <param name="parallelismOptions">Options which determine the amount of test parallelization to allow.</param>
	public async ValueTask<RunSummary> Run(
		ICodeGenTestClass testClass,
		IReadOnlyCollection<ICodeGenTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager collectionFixtureMappings,
		ParallelismOptions parallelismOptions = ParallelismOptionsAliases.Default)
	{
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(cancellationTokenSource);
		Guard.ArgumentNotNull(collectionFixtureMappings);

		await using var ctxt = new CodeGenTestClassRunnerContext(
			testClass,
			testCases,
			explicitOption,
			messageBus,
			aggregator,
			parallelismOptions,
			cancellationTokenSource,
			collectionFixtureMappings
		);
		await ctxt.InitializeAsync();

		return await ctxt.Aggregator.RunAsync(() => Run(ctxt), default);
	}
}
