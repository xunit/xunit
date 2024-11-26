using System.Reflection;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestRunner{TContext, TTest}"/>. This includes an assumption
/// that a test means invoking a method on a class.
/// </summary>
/// <param name="test">The test</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="skipReason">The skip reason for the test, if it's being skipped</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="testMethod">The test method</param>
/// <param name="testMethodArguments">The method arguments for the test method</param>
public class TestRunnerContext<TTest>(
	TTest test,
	IMessageBus messageBus,
	string? skipReason,
	ExplicitOption explicitOption,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	MethodInfo testMethod,
	object?[] testMethodArguments) :
		TestRunnerBaseContext<TTest>(test, messageBus, skipReason, explicitOption, aggregator, cancellationTokenSource)
			where TTest : class, ITest
{
	/// <summary>
	/// Gets the method that this test originated in.
	/// </summary>
	public MethodInfo TestMethod { get; } = Guard.ArgumentNotNull(testMethod);

	/// <summary>
	/// Gets the arguments to be passed to the test method during invocation.
	/// </summary>
	public object?[] TestMethodArguments { get; } = Guard.ArgumentNotNull(testMethodArguments);
}
