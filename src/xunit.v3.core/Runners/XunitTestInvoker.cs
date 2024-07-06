using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test invoker for xUnit.net v3 tests.
/// </summary>
public class XunitTestInvoker : TestInvoker<XunitTestInvokerContext, IXunitTest>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestInvoker"/> class.
	/// </summary>
	protected XunitTestInvoker()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitTestInvoker"/>.
	/// </summary>
	public static XunitTestInvoker Instance = new();

	/// <inheritdoc/>
	protected override ValueTask<TimeSpan> InvokeTestMethodAsync(XunitTestInvokerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return
			ctxt.Test.Timeout > 0
				? InvokeTimeoutTestMethodAsync(ctxt, ctxt.Test.Timeout)
				: base.InvokeTestMethodAsync(ctxt);
	}

	async ValueTask<TimeSpan> InvokeTimeoutTestMethodAsync(
		XunitTestInvokerContext ctxt,
		int timeout)
	{
		// We keep our own notion of execution time, since the time recorded by
		// calling InvokeTestMethodAsync isn't sufficient
		var stopwatch = Stopwatch.StartNew();

		await ctxt.Aggregator.RunAsync(async () =>
		{
			if (!AsyncUtility.IsAsync(ctxt.TestMethod))
				throw TestTimeoutException.ForIncompatibleTest();

			var baseTask = base.InvokeTestMethodAsync(ctxt).AsTask();
			var resultTask = await Task.WhenAny(baseTask, Task.Delay(timeout));

			if (resultTask != baseTask)
			{
				TestContext.Current.Cancel();
				throw TestTimeoutException.ForTimedOutTest(timeout);
			}
		});

		return stopwatch.Elapsed;
	}

	/// <summary>
	/// Creates the test class (if necessary), and invokes the test method.
	/// </summary>
	/// <param name="test">The test that should be run</param>
	/// <param name="testClassInstance">The test class instance</param>
	/// <param name="testMethodArguments">The arguments for the test method</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to send execution messages to</param>
	/// <param name="aggregator">The aggregator used to </param>
	/// <param name="cancellationTokenSource">The cancellation token source used to cancel test execution</param>
	/// <returns>Returns the time (in seconds) spent creating the test class, running
	/// the test, and disposing of the test class.</returns>
	public async ValueTask<TimeSpan> RunAsync(
		IXunitTest test,
		object? testClassInstance,
		object?[] testMethodArguments,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource)
	{
		await using var ctxt = new XunitTestInvokerContext(explicitOption, messageBus, aggregator, cancellationTokenSource, test, testClassInstance, testMethodArguments);
		await ctxt.InitializeAsync();

		return await RunAsync(ctxt);
	}
}
