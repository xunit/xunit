using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestClassRunner"/>.
/// </summary>
/// <param name="testClass">The test class</param>
/// <param name="testCases">The test from the test class</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="parallelMode">The parallel mode for the test class</param>
/// <param name="scheduler">The scheduler used for task/test scheduling</param>
/// <param name="collectionFixtureMappings">The fixtures attached to the test collection</param>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestClassRunnerContext(
	IXunitTestClass testClass,
	IReadOnlyCollection<IXunitTestCase> testCases,
	ExplicitOption explicitOption,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	ParallelMode parallelMode,
	ExecutionScheduler scheduler,
	FixtureMappingManager collectionFixtureMappings) :
		XunitTestClassRunnerBaseContext<IXunitTestClass, IXunitTestMethod, IXunitTestCase>(testClass, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler, collectionFixtureMappings)
{
	/// <summary>
	/// Please call <see cref="XunitTestClassRunnerContext(IXunitTestClass, IReadOnlyCollection{IXunitTestCase}, ExplicitOption, IMessageBus, ExceptionAggregator, CancellationTokenSource, ParallelMode, ExecutionScheduler, FixtureMappingManager)"/>.
	/// This constructor is no longer valid and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call the constructor which removes testCaseOrderer and adds parallelMode and scheduler. This overload is no longer valid and will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public XunitTestClassRunnerContext(
		IXunitTestClass testClass,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager collectionFixtureMappings) :
			this(testClass, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, ParallelMode.None, ExecutionScheduler.Invalid, collectionFixtureMappings) =>
				throw new NotSupportedException("Please call the constructor which removes testCaseOrderer and adds parallelMode and scheduler. This overload is no longer valid and will be removed in the next major version.");
}
