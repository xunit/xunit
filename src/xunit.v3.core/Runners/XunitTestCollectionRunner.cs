using System;
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
	TestCollectionRunner<XunitTestCollectionRunnerContext, IXunitTestCollection, IXunitTestClass, IXunitTestCase>
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
	/// Gives an opportunity to override test case orderer. By default, this method gets the
	/// orderer from the collection definition. If this function returns <c>null</c>, the
	/// test case orderer passed into the constructor will be used.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test collection</param>
	protected virtual ITestCaseOrderer? GetTestCaseOrderer(XunitTestCollectionRunnerContext ctxt) =>
		Guard.ArgumentNotNull(ctxt).TestCollection.TestCaseOrderer;

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestCollectionCleanupFailure(
		XunitTestCollectionRunnerContext ctxt,
		Exception exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var (types, messages, stackTraces, indices, _) = ExceptionUtility.ExtractMetadata(exception);

		return new(ctxt.MessageBus.QueueMessage(new TestCollectionCleanupFailure
		{
			AssemblyUniqueID = ctxt.TestCollection.TestAssembly.UniqueID,
			ExceptionParentIndices = indices,
			ExceptionTypes = types,
			Messages = messages,
			StackTraces = stackTraces,
			TestCollectionUniqueID = ctxt.TestCollection.UniqueID,
		}));
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestCollectionFinished(
		XunitTestCollectionRunnerContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.CollectionFixtureMappings.DisposeAsync);

		return ctxt.MessageBus.QueueMessage(new TestCollectionFinished
		{
			AssemblyUniqueID = ctxt.TestCollection.TestAssembly.UniqueID,
			ExecutionTime = summary.Time,
			TestCollectionUniqueID = ctxt.TestCollection.UniqueID,
			TestsFailed = summary.Failed,
			TestsNotRun = summary.NotRun,
			TestsSkipped = summary.Skipped,
			TestsTotal = summary.Total,
		});
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestCollectionStarting(XunitTestCollectionRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = ctxt.MessageBus.QueueMessage(new TestCollectionStarting
		{
			AssemblyUniqueID = ctxt.TestCollection.TestAssembly.UniqueID,
			TestCollectionClassName = Guard.ArgumentNotNull(ctxt).TestCollection.TestCollectionClassName,
			TestCollectionDisplayName = ctxt.TestCollection.TestCollectionDisplayName,
			TestCollectionUniqueID = ctxt.TestCollection.UniqueID,
			Traits = ctxt.TestCollection.Traits,
		});

		await ctxt.Aggregator.RunAsync(() => ctxt.CollectionFixtureMappings.InitializeAsync(ctxt.TestCollection.CollectionFixtureTypes));

		return result;
	}

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
	public async ValueTask<RunSummary> RunAsync(
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

		return await RunAsync(ctxt);
	}

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestClassAsync(
		XunitTestCollectionRunnerContext ctxt,
		IXunitTestClass? testClass,
		IReadOnlyCollection<IXunitTestCase> testCases,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(testCases);

		return
			exception is not null
				? new(XunitRunnerHelper.FailTestCases(ctxt.MessageBus, ctxt.CancellationTokenSource, testCases, exception, sendTestClassMessages: true, sendTestMethodMessages: true))
				: testClass is null
					? new(XunitRunnerHelper.FailTestCases(ctxt.MessageBus, ctxt.CancellationTokenSource, testCases, "Test case {0} does not have an associated class and cannot be run by XunitTestClassRunner", sendTestClassMessages: true, sendTestMethodMessages: true))
					: XunitTestClassRunner.Instance.RunAsync(
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
