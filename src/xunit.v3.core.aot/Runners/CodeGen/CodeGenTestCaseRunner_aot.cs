using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test case runner for xUnit.net v3 tests.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestCaseRunner : CoreTestCaseRunner<CodeGenTestCaseRunnerContext, ICodeGenTestCase, ICodeGenTest>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="CodeGenTestCaseRunner"/> class.
	/// </summary>
	protected CodeGenTestCaseRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="CodeGenTestCaseRunner"/> class.
	/// </summary>
	public static CodeGenTestCaseRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test case.
	/// </summary>
	/// <remarks>
	/// This entry point is used for both single-test (like <see cref="FactAttribute"/> and individual data
	/// rows for <see cref="TheoryAttribute"/> tests) and multi-test test cases (like <see cref="TheoryAttribute"/>
	/// when pre-enumeration is disable or the theory data was not serializable).
	/// </remarks>
	/// <param name="testCase">The test case that this invocation belongs to.</param>
	/// <param name="tests">The tests for the test case.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="displayName">The display name of the test case.</param>
	/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="classFixtureMappings">The mapping of class fixture types to fixtures.</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	public async ValueTask<RunSummary> Run(
		ICodeGenTestCase testCase,
		IReadOnlyCollection<ICodeGenTest> tests,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		string displayName,
		string? skipReason,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager classFixtureMappings)
	{
		await using var ctxt = new CodeGenTestCaseRunnerContext(
			testCase,
			tests,
			explicitOption,
			messageBus,
			aggregator,
			displayName,
			skipReason,
			cancellationTokenSource,
			classFixtureMappings
		);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}
}
