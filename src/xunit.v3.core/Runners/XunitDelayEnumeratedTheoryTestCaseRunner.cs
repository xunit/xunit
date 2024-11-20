using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test case runner for xUnit.net v3 theories (which could not be pre-enumerated;
/// pre-enumerated test cases use <see cref="XunitTestCaseRunner"/>).
/// </summary>
public class XunitDelayEnumeratedTheoryTestCaseRunner :
	XunitDelayEnumeratedTestCaseRunnerBase<XunitDelayEnumeratedTestCaseRunnerContext<IXunitDelayEnumeratedTestCase>, IXunitDelayEnumeratedTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitDelayEnumeratedTheoryTestCaseRunner"/> class.
	/// </summary>
	protected XunitDelayEnumeratedTheoryTestCaseRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitDelayEnumeratedTheoryTestCaseRunner"/> class.
	/// </summary>
	public static XunitDelayEnumeratedTheoryTestCaseRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test case.
	/// </summary>
	/// <param name="testCase">The test case that this invocation belongs to.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
	/// <param name="displayName">The display name of the test case.</param>
	/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	public async ValueTask<RunSummary> RunAsync(
		IXunitDelayEnumeratedTestCase testCase,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		string displayName,
		string? skipReason,
		ExplicitOption explicitOption,
		object?[] constructorArguments)
	{
		Guard.ArgumentNotNull(testCase);
		Guard.ArgumentNotNull(displayName);
		Guard.ArgumentNotNull(constructorArguments);

		await using var ctxt = new XunitDelayEnumeratedTestCaseRunnerContext<IXunitDelayEnumeratedTestCase>(testCase, messageBus, aggregator, cancellationTokenSource, displayName, skipReason, explicitOption, constructorArguments);
		await ctxt.InitializeAsync();

		return await RunAsync(ctxt);
	}
}
