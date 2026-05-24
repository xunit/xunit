using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CodeGenTestCaseRunnerBase{TContext, TTestCase, TTest}"/>.
/// </summary>
/// <param name="testCase">The test case</param>
/// <param name="tests">The tests for the test case</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="displayName">The display name of the test case</param>
/// <param name="skipReason">The skip reason, if the test case is being skipped</param>
/// <param name="parallelismOptions">Options which determine the amount of test parallelization to allow.</param>
/// <param name="parallelizationSemaphore">Semaphore used to limit the number of tests running in parallel.</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="methodFixtureMappings">The mapping of method fixture types to fixtures.</param>
/// <remarks>
/// This class is used for code generation-based tests.
/// </remarks>
public abstract class CodeGenTestCaseRunnerBaseContext<TTestCase, TTest>(
	TTestCase testCase,
	IReadOnlyCollection<TTest> tests,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	string displayName,
	string? skipReason,
	ParallelismOptions parallelismOptions,
	ITestPipelineSemaphore? parallelizationSemaphore,
	CancellationTokenSource cancellationTokenSource,
	FixtureMappingManager methodFixtureMappings) :
	CoreTestCaseRunnerContext<TTestCase, TTest>(
		testCase,
		tests,
		explicitOption,
		messageBus,
		aggregator,
		displayName,
		skipReason,
		parallelismOptions,
		parallelizationSemaphore,
		cancellationTokenSource)
	where TTestCase : class, ICodeGenTestCase
	where TTest : class, ICodeGenTest
{
	/// <summary>
	/// Gets the mapping manager for case-level fixtures.
	/// </summary>
	/// <remarks>
	/// There is no mechanism for describing or attaching case-level fixtures at this time, so this currently
	/// returns the mapping manager for the class-level fixtures. If case-level fixtures become a feature in the
	/// future, it is anticipated that this API will return the case-level fixture mapping manager.
	/// </remarks>
	public FixtureMappingManager CaseFixtureMappings { get; } = Guard.ArgumentNotNull(methodFixtureMappings);
}
