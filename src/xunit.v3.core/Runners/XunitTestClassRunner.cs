using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test class runner for xUnit.net v3 tests.
/// </summary>
public class XunitTestClassRunner :
	XunitTestClassRunnerBase<XunitTestClassRunnerContext, IXunitTestClass, IXunitTestMethod, IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestClassRunner"/> class.
	/// </summary>
	protected XunitTestClassRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitTestClassRunner"/> class.
	/// </summary>
	public static XunitTestClassRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test class.
	/// </summary>
	/// <param name="testClass">The test class to be run.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the test.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="collectionFixtureMappings">The mapping of collection fixture types to fixtures.</param>
	/// <returns></returns>
	public async ValueTask<RunSummary> Run(
		IXunitTestClass testClass,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager collectionFixtureMappings)
	{
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(testCaseOrderer);
		Guard.ArgumentNotNull(cancellationTokenSource);
		Guard.ArgumentNotNull(collectionFixtureMappings);

		await using var ctxt = new XunitTestClassRunnerContext(testClass, @testCases, explicitOption, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings);
		await ctxt.InitializeAsync();

		return await ctxt.Aggregator.RunAsync(() => Run(ctxt), default);
	}

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestMethod(
		XunitTestClassRunnerContext ctxt,
		IXunitTestMethod? testMethod,
		IReadOnlyCollection<IXunitTestCase> testCases,
		object?[] constructorArguments)
	{
		Guard.ArgumentNotNull(ctxt);

		// Technically not possible because of the design of TTestClass, but this signature is imposed
		// by the base class, which allows method-less tests
		return
			testMethod is null
				? new(XunitRunnerHelper.FailTestCases(ctxt.MessageBus, ctxt.CancellationTokenSource, testCases, "Test case '{0}' does not have an associated method and cannot be run by XunitTestMethodRunner", sendTestMethodMessages: true))
				: XunitTestMethodRunner.Instance.Run(
					testMethod,
					testCases,
					ctxt.ExplicitOption,
					ctxt.MessageBus,
					ctxt.Aggregator.Clone(),
					ctxt.CancellationTokenSource,
					constructorArguments
				);
	}
}
