using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CodeGenTestCaseRunner"/>.
/// </summary>
/// <param name="testCase">The test case</param>
/// <param name="tests">The tests for the test case</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="displayName">The display name of the test case</param>
/// <param name="skipReason">The skip reason, if the test case is being skipped</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="classFixtureMappings">The mapping of class fixture types to fixtures.</param>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public class CodeGenTestCaseRunnerContext(
	ICodeGenTestCase testCase,
	IReadOnlyCollection<ICodeGenTest> tests,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	string displayName,
	string? skipReason,
	CancellationTokenSource cancellationTokenSource,
	FixtureMappingManager classFixtureMappings) :
		CoreTestCaseRunnerContext<ICodeGenTestCase, ICodeGenTest>(testCase, tests, explicitOption, messageBus, aggregator, displayName, skipReason, cancellationTokenSource)
{
	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunTest(ICodeGenTest test) =>
		CodeGenTestRunner.Instance.Run(
			test,
			MessageBus,
			ExplicitOption,
			Aggregator.Clone(),
			CancellationTokenSource,
			classFixtureMappings
		);
}
