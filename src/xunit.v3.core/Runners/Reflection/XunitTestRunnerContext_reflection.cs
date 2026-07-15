using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestRunner"/>.
/// </summary>
/// <param name="test">The test</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="parallelMode">The parallel mode for the test</param>
/// <param name="scheduler">The scheduler used for task/test scheduling</param>
/// <param name="beforeAfterTestAttributes">The <see cref="IBeforeAfterTestAttribute"/>s that are applied to the test</param>
/// <param name="constructorArguments">The constructor arguments for the test class</param>
/// <param name="caseFixtureMappings">The fixtures attached to the test case</param>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestRunnerContext(
	IXunitTest test,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	ParallelMode parallelMode,
	ExecutionScheduler scheduler,
	IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterTestAttributes,
	object?[] constructorArguments,
	FixtureMappingManager caseFixtureMappings) :
		XunitTestRunnerBaseContext<IXunitTest>(test, explicitOption, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler, beforeAfterTestAttributes, constructorArguments, caseFixtureMappings)
{
	/// <summary>
	/// Please call <see cref="XunitTestRunnerContext(IXunitTest, ExplicitOption, IMessageBus, ExceptionAggregator, CancellationTokenSource, ParallelMode, ExecutionScheduler, IReadOnlyCollection{IBeforeAfterTestAttribute}, object?[], FixtureMappingManager)"/>.
	/// This overload is no longer valid and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call the constructor which adds parallelMode, scheduler, and caseFixtureMappings. This overload is no longer valid and will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public XunitTestRunnerContext(
		IXunitTest test,
		IMessageBus messageBus,
		ExplicitOption explicitOption,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterTestAttributes,
		object?[] constructorArguments) :
			this(test, explicitOption, messageBus, aggregator, cancellationTokenSource, ParallelMode.None, ExecutionScheduler.Invalid, beforeAfterTestAttributes, constructorArguments, FixtureMappingManager.Empty) =>
				throw new NotSupportedException("Please call the constructor which adds parallelMode, scheduler, and caseFixtureMappings. This overload is no longer valid and will be removed in the next major version.");
}
