using System.ComponentModel;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestCaseRunnerBase{TContext, TTestCase, TTest}"/>.
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
public class XunitTestCaseRunnerBaseContext<TTestCase, TTest>(
	TTestCase testCase,
	IReadOnlyCollection<TTest> tests,
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
		CoreTestCaseRunnerContext<TTestCase, TTest>(testCase, tests, explicitOption, messageBus, aggregator, displayName, skipReason, cancellationTokenSource, parallelMode, scheduler)
			where TTestCase : class, IXunitTestCase
			where TTest : class, IXunitTest
{
	/// <summary>
	/// Please call <see cref="XunitTestCaseRunnerBaseContext(TTestCase, IReadOnlyCollection{TTest}, ExplicitOption, IMessageBus, ExceptionAggregator, string, string?, CancellationTokenSource, ParallelMode, ExecutionScheduler, object?[], FixtureMappingManager)"/>.
	/// This overload is no longer valid and will be removed in the next major version.
	/// </summary>
	[Obsolete("Please call the constructor which adds parallelMode, scheduler, and methodFixtureMappings. This overload is no longer valid and will be removed in the next major version.", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public XunitTestCaseRunnerBaseContext(
		TTestCase testCase,
		IReadOnlyCollection<TTest> tests,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		string displayName,
		string? skipReason,
		ExplicitOption explicitOption,
		object?[] constructorArguments) :
			this(testCase, tests, explicitOption, messageBus, aggregator, displayName, skipReason, cancellationTokenSource, ParallelMode.None, ExecutionScheduler.Invalid, constructorArguments, FixtureMappingManager.Empty) =>
				throw new NotSupportedException("Please call the constructor which adds parallelMode, scheduler, and methodFixtureMappings. This overload is no longer valid and will be removed in the next major version.");

	/// <summary>
	/// Gets the list of <see cref="IBeforeAfterTestAttribute"/> instances for this test case.
	/// </summary>
	public IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes =>
		TestCase.TestMethod.BeforeAfterTestAttributes;

	/// <summary>
	/// Gets the mapping manager for case-level fixtures.
	/// </summary>
	/// <remarks>
	/// There is no mechanism for describing or attaching case-level fixtures at this time, so this currently
	/// returns the mapping manager for the class-level fixtures. If case-level fixtures become a feature in the
	/// future, it is anticipated that this API will return the case-level fixture mapping manager.
	/// </remarks>
	public FixtureMappingManager CaseFixtureMappings { get; } = Guard.ArgumentNotNull(methodFixtureMappings);

	/// <summary>
	/// Gets the arguments to pass to the constructor of the test class when creating it.
	/// </summary>
	public object?[] ConstructorArguments { get; } = Guard.ArgumentNotNull(constructorArguments);

	/// <inheritdoc/>
	public override ValueTask<RunSummary> RunTest(TTest test)
	{
		Guard.ArgumentNotNull(test);

		return XunitTestRunner.Instance.Run(
			test,
			MessageBus,
			ConstructorArguments,
			ExplicitOption,
			Aggregator.Clone(),
			CancellationTokenSource,
			ParallelMode,
			Scheduler,
			BeforeAfterTestAttributes,
			CaseFixtureMappings
		);
	}
}
