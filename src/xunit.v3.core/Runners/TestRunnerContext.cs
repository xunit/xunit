using System;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Base context class for <see cref="TestRunner{TContext, TTest}"/>.
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
		ContextBase(explicitOption, messageBus, aggregator, cancellationTokenSource)
			where TTest : class, ITest
{
	/// <summary>
	/// Gets the skip reason given for the test; will be <c>null</c> if the test is not
	/// statically skipped. (It may still be dynamically skipped at a later time.)
	/// </summary>
	public string? SkipReason { get; } = skipReason;

	/// <summary>
	/// Gets the test that's being invoked.
	/// </summary>
	public TTest Test { get; } = Guard.ArgumentNotNull(test);

	/// <summary>
	/// Gets the runtime skip reason for the test.
	/// </summary>
	/// <param name="exception">The exception that was thrown during test invocation</param>
	/// <returns>The skip reason, if the test is skipped; <c>null</c>, otherwise</returns>
	public virtual string? GetSkipReason(Exception? exception = null) =>
		SkipReason;
}
