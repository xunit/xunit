using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestMethodRunner"/>.
/// </summary>
/// <param name="testMethod">The test method</param>
/// <param name="testCases">The test cases from the test method</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="parallelMode">The parallel mode for the test method</param>
/// <param name="scheduler">The scheduler used for task/test scheduling</param>
/// <param name="constructorArguments">The constructor arguments for the test class</param>
/// <param name="classFixtureMappings">The fixtures attached to the test class</param>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestMethodRunnerContext(
	IXunitTestMethod testMethod,
	IReadOnlyCollection<IXunitTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	ParallelMode parallelMode,
	ExecutionScheduler scheduler,
	object?[] constructorArguments,
	FixtureMappingManager classFixtureMappings) :
		XunitTestMethodRunnerBaseContext<IXunitTestMethod, IXunitTestCase>(testMethod, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler, constructorArguments, classFixtureMappings)
{
	/// <summary>
	/// Please call <see cref="XunitTestMethodRunnerContext(IXunitTestMethod, IReadOnlyCollection{IXunitTestCase}, ExplicitOption, IMessageBus, ExceptionAggregator, CancellationTokenSource, ParallelMode, ExecutionScheduler, object?[], FixtureMappingManager)"/>.
	/// This overload is no longer valid and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call the constructor which adds parallelMode, scheduler, and classFixtureMappings. This overload is no longer valid and will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public XunitTestMethodRunnerContext(
		IXunitTestMethod testMethod,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		object?[] constructorArguments) :
			this(testMethod, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, ParallelMode.None, ExecutionScheduler.Invalid, constructorArguments, FixtureMappingManager.Empty) =>
				throw new NotSupportedException("Please call the constructor which adds parallelMode, scheduler, and classFixtureMappings. This overload is no longer valid and will be removed in the next major version.");
}
