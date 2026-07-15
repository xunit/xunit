using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CodeGenTestMethodRunner"/>.
/// </summary>
/// <param name="testMethod">The test method</param>
/// <param name="testCases">The test cases from the test method</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="parallelMode">The parallel mode for the test method</param>
/// <param name="scheduler">The scheduler used for task/test scheduling</param>
/// <param name="classFixtureMappings">The mapping of class fixture types to fixtures.</param>
public class CodeGenTestMethodRunnerContext(
	ICodeGenTestMethod testMethod,
	IReadOnlyCollection<ICodeGenTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	ParallelMode parallelMode,
	ExecutionScheduler scheduler,
	FixtureMappingManager classFixtureMappings) :
		CodeGenTestMethodRunnerBaseContext<ICodeGenTestMethod, ICodeGenTestCase>(testMethod, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler, classFixtureMappings)
{
	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunTestCase(ICodeGenTestCase testCase)
	{
		if (testCase is ISelfExecutingCodeGenTestCase selfExecutingTestCase)
			return selfExecutingTestCase.Run(ExplicitOption, MessageBus, Aggregator.Clone(), CancellationTokenSource, ParallelMode, Scheduler, MethodFixtureMappings);

		return XunitRunnerHelper.RunCodeGenTestCase(
			testCase,
			MessageBus,
			CancellationTokenSource,
			ParallelMode,
			Scheduler,
			Aggregator.Clone(),
			ExplicitOption,
			MethodFixtureMappings
		);
	}
}
