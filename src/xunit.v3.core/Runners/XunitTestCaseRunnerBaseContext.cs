using System.Collections.Generic;
using System.Threading;
using Xunit.Internal;
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
		TestCaseRunnerContext<TTestCase, TTest>(testCase, explicitOption, messageBus, aggregator, cancellationTokenSource)
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
	public object?[] ConstructorArguments { get; } = Guard.ArgumentNotNull(constructorArguments);

	/// <summary>
	/// Gets the display name of the test case.
	/// </summary>
	public string DisplayName { get; } = Guard.ArgumentNotNullOrEmpty(displayName);

	/// <summary>
	/// Gets the statically specified skip reason for the test. Note that this only covers values
	/// passed via <see cref="IFactAttribute.Skip"/>, and not dynamically skipped tests.
	/// </summary>
	public string? SkipReason { get; } = skipReason;

	/// <inheritdoc/>
	public override IReadOnlyCollection<TTest> Tests { get; } = Guard.ArgumentNotNull(tests);
}
