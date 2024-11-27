using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test case runner for xUnit.net v3 tests.
/// </summary>
public class XunitTestCaseRunner :
	XunitTestCaseRunnerBase<XunitTestCaseRunnerContext, IXunitTestCase, IXunitTest>
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
	/// <remarks>
	/// This entry point is used for both single-test (like <see cref="FactAttribute"/> and individual data
	/// rows for <see cref="TheoryAttribute"/> tests) and multi-test test cases (like <see cref="TheoryAttribute"/>
	/// when pre-enumeration is disable or the theory data was not serializable).
	/// </remarks>
	/// <param name="testCase">The test case that this invocation belongs to.</param>
	/// <param name="tests">The tests for the test case.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="displayName">The display name of the test case.</param>
	/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	public async ValueTask<RunSummary> Run(
		IXunitTestCase testCase,
		IReadOnlyCollection<IXunitTest> tests,
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

		await using var ctxt = new XunitTestCaseRunnerContext(
			testCase,
			tests,
			messageBus,
			aggregator,
			cancellationTokenSource,
			displayName,
			skipReason,
			explicitOption,
			constructorArguments
		);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTest(
		XunitTestCaseRunnerContext ctxt,
		IXunitTest test)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(test);

		return XunitTestRunner.Instance.Run(
			test,
			ctxt.MessageBus,
			ctxt.ConstructorArguments,
			ctxt.ExplicitOption,
			ctxt.Aggregator.Clone(),
			ctxt.CancellationTokenSource,
			ctxt.BeforeAfterTestAttributes
		);
	}
}
