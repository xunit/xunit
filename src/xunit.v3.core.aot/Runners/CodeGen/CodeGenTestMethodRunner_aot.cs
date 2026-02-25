using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test method runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestMethodRunner : CoreTestMethodRunner<CodeGenTestMethodRunnerContext, ICodeGenTestMethod, ICodeGenTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CodeGenTestMethodRunner"/> class.
	/// </summary>
	protected CodeGenTestMethodRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="CodeGenTestMethodRunner"/> class.
	/// </summary>
	public static CodeGenTestMethodRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test test method.
	/// </summary>
	/// <param name="testMethod">The test method to be run.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="classFixtureMappings">The mapping of class fixture types to fixtures.</param>
	public async ValueTask<RunSummary> Run(
		ICodeGenTestMethod testMethod,
		IReadOnlyCollection<ICodeGenTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager classFixtureMappings)
	{
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);

		await using var ctxt = new CodeGenTestMethodRunnerContext(
			Guard.ArgumentNotNull(testMethod),
			testCases,
			explicitOption,
			messageBus,
			aggregator,
			cancellationTokenSource,
			classFixtureMappings
		);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}
}
