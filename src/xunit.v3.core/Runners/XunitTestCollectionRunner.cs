using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test collection runner for xUnit.net v3 tests.
/// </summary>
public class XunitTestCollectionRunner :
	XunitTestCollectionRunnerBase<XunitTestCollectionRunnerContext, IXunitTestCollection, IXunitTestClass, IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestCollectionRunner"/> class.
	/// </summary>
	protected XunitTestCollectionRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of <see cref="XunitTestCollectionRunner"/>.
	/// </summary>
	public static XunitTestCollectionRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test collection.
	/// </summary>
	/// <param name="testCollection">The test collection to be run.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="testCaseOrderer">The test case orderer that was applied at the assembly level.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="assemblyFixtureMappings">The mapping manager for assembly fixtures.</param>
	public async ValueTask<RunSummary> Run(
		IXunitTestCollection testCollection,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager assemblyFixtureMappings)
	{
		Guard.ArgumentNotNull(testCollection);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(testCaseOrderer);
		Guard.ArgumentNotNull(cancellationTokenSource);
		Guard.ArgumentNotNull(assemblyFixtureMappings);

		await using var ctxt = new XunitTestCollectionRunnerContext(testCollection, testCases, explicitOption, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, assemblyFixtureMappings);
		await ctxt.InitializeAsync();

		return await Run(ctxt);
	}

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestClass(
		XunitTestCollectionRunnerContext ctxt,
		IXunitTestClass? testClass,
		IReadOnlyCollection<IXunitTestCase> testCases)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(testCases);

		if (testClass is null)
			return new(XunitRunnerHelper.FailTestCases(
				ctxt.MessageBus,
				ctxt.CancellationTokenSource,
				testCases,
				"Test case '{0}' does not have an associated class and cannot be run by XunitTestClassRunner",
				sendTestClassMessages: true,
				sendTestMethodMessages: true
			));

		return
			XunitTestClassRunner.Instance.Run(
				testClass,
				testCases,
				ctxt.ExplicitOption,
				ctxt.MessageBus,
				ctxt.TestCaseOrderer,
				ctxt.Aggregator.Clone(),
				ctxt.CancellationTokenSource,
				ctxt.CollectionFixtureMappings
			);
	}
}
