using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CodeGenTestRunner"/>.
/// </summary>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
/// <param name="test">The test</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="caseFixtureMappings">The mapping of test case fixture types to fixtures.</param>
public class CodeGenTestRunnerContext(
	ICodeGenTest test,
	IMessageBus messageBus,
	ExplicitOption explicitOption,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	FixtureMappingManager caseFixtureMappings) :
		CodeGenTestRunnerBaseContext<ICodeGenTest>(test, messageBus, explicitOption, aggregator, cancellationTokenSource, caseFixtureMappings)
{
	/// <inheritdoc/>
	public override ValueTask<TimeSpan> InvokeTest(object? testClassInstance) =>
		ExecutionTimer.MeasureAsync(() => Test.MethodInvoker(testClassInstance));
}
