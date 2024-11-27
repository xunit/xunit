using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test runner for xUnit.net v3 tests.
/// </summary>
public class XunitTestRunner : XunitTestRunnerBase<XunitTestRunnerContext, IXunitTest>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestRunner"/> class.
	/// </summary>
	protected XunitTestRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitTestRunner"/>.
	/// </summary>
	public static XunitTestRunner Instance = new();

	/// <summary>
	/// Runs the test.
	/// </summary>
	/// <param name="test">The test that this invocation belongs to.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="beforeAfterAttributes">The list of <see cref="IBeforeAfterTestAttribute"/>s for this test.</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	public async ValueTask<RunSummary> Run(
		IXunitTest test,
		IMessageBus messageBus,
		object?[] constructorArguments,
		ExplicitOption explicitOption,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterAttributes)
	{
		await using var ctxt = new XunitTestRunnerContext(
			test,
			messageBus,
			explicitOption,
			aggregator,
			cancellationTokenSource,
			beforeAfterAttributes,
			constructorArguments
		);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}
}
