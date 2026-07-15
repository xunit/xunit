using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestCaseRunner"/>.
/// </summary>
/// <param name="testCase">The test case</param>
/// <param name="tests">The tests for the test case</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="displayName">The display name of the test case</param>
/// <param name="skipReason">The skip reason, if the test case is being skipped</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="parallelMode">The parallel mode for the test case</param>
/// <param name="scheduler">The scheduler used for task/test scheduling</param>
/// <param name="constructorArguments">The constructor arguments for the test class</param>
/// <param name="methodFixtureMappings">The fixtures attached to the test method</param>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestCaseRunnerContext(
	IXunitTestCase testCase,
	IReadOnlyCollection<IXunitTest> tests,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	string displayName,
	string? skipReason,
	CancellationTokenSource cancellationTokenSource,
	ParallelMode parallelMode,
	ExecutionScheduler scheduler,
	object?[] constructorArguments,
	FixtureMappingManager methodFixtureMappings) :
		XunitTestCaseRunnerBaseContext<IXunitTestCase, IXunitTest>(testCase, tests, explicitOption, messageBus, aggregator, displayName, skipReason, cancellationTokenSource, parallelMode, scheduler, constructorArguments, methodFixtureMappings)
{
	/// <summary>
	/// Please call <see cref="XunitTestCaseRunnerContext(IXunitTestCase, IReadOnlyCollection{IXunitTest}, ExplicitOption, IMessageBus, ExceptionAggregator, string, string?, CancellationTokenSource, ParallelMode, ExecutionScheduler, object?[], FixtureMappingManager)"/>.
	/// This constructor is no longer valid and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call the constructor which adds parallelMode, scheduler, and methodFixtureMappings. This constructor is no longer valid and will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public XunitTestCaseRunnerContext(
		IXunitTestCase testCase,
		IReadOnlyCollection<IXunitTest> tests,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		string displayName,
		string? skipReason,
		ExplicitOption explicitOption,
		object?[] constructorArguments) :
			this(testCase, tests, explicitOption, messageBus, aggregator, displayName, skipReason, cancellationTokenSource, ParallelMode.None, ExecutionScheduler.Invalid, constructorArguments, FixtureMappingManager.Empty) =>
				throw new NotSupportedException("Please call the constructor which adds parallelMode, scheduler, and methodFixtureMappings. This constructor is no longer valid and will be removed in the next major version.");
}
