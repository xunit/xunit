using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test case runner for xUnit.net v3 tests.
/// </summary>
public class XunitTestCaseRunner : XunitTestCaseRunnerBase<XunitTestCaseRunnerContext>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestCaseRunner"/> class.
	/// </summary>
	protected XunitTestCaseRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitTestCaseRunner"/> class.
	/// </summary>
	public static XunitTestCaseRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test case.
	/// </summary>
	/// <param name="testCase">The test case that this invocation belongs to.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="displayName">The display name of the test case.</param>
	/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
	/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
	/// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	public ValueTask<RunSummary> RunAsync(
		IXunitTestCase testCase,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		string displayName,
		string? skipReason,
		object?[] constructorArguments,
		object?[]? testMethodArguments)
	{
		Guard.ArgumentNotNull(testCase);
		Guard.ArgumentNotNull(displayName);
		Guard.ArgumentNotNull(constructorArguments);

		var (testClass, testMethod, beforeAfterTestAttributes) = Initialize(testCase, ref testMethodArguments);

		return RunAsync(new(testCase, messageBus, aggregator, cancellationTokenSource, displayName, skipReason, testClass, constructorArguments, testMethod, testMethodArguments, beforeAfterTestAttributes));
	}
}
