using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestCaseRunnerBase{TContext, TTestCase, TTest}"/>.
/// </summary>
/// <param name="testCase">The test case</param>
/// <param name="tests">The tests for the test case</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="displayName">The display name of the test case</param>
/// <param name="skipReason">The skip reason, if the test case is being skipped</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="constructorArguments">The constructor arguments for the test class</param>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public class XunitTestCaseRunnerBaseContext<TTestCase, TTest>(
	TTestCase testCase,
	IReadOnlyCollection<TTest> tests,
	IMessageBus messageBus,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	string displayName,
	string? skipReason,
	ExplicitOption explicitOption,
	object?[] constructorArguments) :
		CoreTestCaseRunnerContext<TTestCase, TTest>(testCase, tests, explicitOption, messageBus, aggregator, displayName, skipReason, cancellationTokenSource)
			where TTestCase : class, IXunitTestCase
			where TTest : class, IXunitTest
{
	/// <summary>
	/// Gets the list of <see cref="IBeforeAfterTestAttribute"/> instances for this test case.
	/// </summary>
	public IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes =>
		TestCase.TestMethod.BeforeAfterTestAttributes;

	/// <summary>
	/// Gets the arguments to pass to the constructor of the test class when creating it.
	/// </summary>
	public object?[] ConstructorArguments { get; } =
		Guard.ArgumentNotNull(constructorArguments);

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
			BeforeAfterTestAttributes
		);
	}
}
