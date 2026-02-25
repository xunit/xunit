using System.ComponentModel;
using System.Reflection;
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
public class TestRunnerContext<TTest>(
	TTest test,
	IMessageBus messageBus,
	string? skipReason,
	ExplicitOption explicitOption,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource) :
		TestRunnerBaseContext<TTest>(test, messageBus, skipReason, explicitOption, aggregator, cancellationTokenSource)
			where TTest : class, ITest
{
#if XUNIT_AOT
	/// <summary>
	/// Test method invocation has been moved to <see cref="CodeGenTestRunnerContext"/>.
	/// Please call <see cref="TestRunnerContext{TTest}.TestRunnerContext(TTest, IMessageBus, string?, ExplicitOption, ExceptionAggregator, CancellationTokenSource)"/> instead.
	/// </summary>
	[Obsolete("Test method invocation has been moved to CodeGenTestRunnerContext; please use the overload without testMethod or testMethodArguments")]
#else
	/// <summary>
	/// Test method invocation has been moved to <see cref="XunitTestRunnerBaseContext{TTest}"/>.
	/// Please call <see cref="TestRunnerContext{TTest}.TestRunnerContext(TTest, IMessageBus, string?, ExplicitOption, ExceptionAggregator, CancellationTokenSource)"/> instead.
	/// </summary>
	[Obsolete("Test method invocation has been moved to XunitTestRunnerBaseContext; please use the overload without testMethod or testMethodArguments")]
#endif
	[EditorBrowsable(EditorBrowsableState.Never)]
	public TestRunnerContext(
		TTest test,
		IMessageBus messageBus,
		string? skipReason,
		ExplicitOption explicitOption,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		MethodInfo testMethod,
		object?[] testMethodArguments) :
			this(test, messageBus, skipReason, explicitOption, aggregator, cancellationTokenSource)
	{ }
}
