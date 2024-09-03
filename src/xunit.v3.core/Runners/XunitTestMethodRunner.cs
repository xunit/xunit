using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test method runner for xUnit.net v3 tests.
/// </summary>
public class XunitTestMethodRunner :
	TestMethodRunner<XunitTestMethodRunnerContext, IXunitTestMethod, IXunitTestCase>
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitTestMethodRunner"/> class.
	/// </summary>
	public static XunitTestMethodRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test test method.
	/// </summary>
	/// <param name="testMethod">The test method to be run.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="constructorArguments">The constructor arguments for the test class.</param>
	public async ValueTask<RunSummary> RunAsync(
		IXunitTestMethod testMethod,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		object?[] constructorArguments)
	{
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(constructorArguments);

		await using var ctxt = new XunitTestMethodRunnerContext(testMethod, testCases, explicitOption, messageBus, aggregator, cancellationTokenSource, constructorArguments);
		await ctxt.InitializeAsync();

		return await RunAsync(ctxt);
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestMethodCleanupFailure(
		XunitTestMethodRunnerContext ctxt,
		Exception exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var (types, messages, stackTraces, indices, _) = ExceptionUtility.ExtractMetadata(exception);

		return new(ctxt.MessageBus.QueueMessage(new TestMethodCleanupFailure
		{
			AssemblyUniqueID = ctxt.TestMethod.TestClass.TestCollection.TestAssembly.UniqueID,
			ExceptionParentIndices = indices,
			ExceptionTypes = types,
			Messages = messages,
			StackTraces = stackTraces,
			TestClassUniqueID = ctxt.TestMethod.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestMethod.TestClass.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.TestMethod.UniqueID,
		}));
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestMethodFinished(
		XunitTestMethodRunnerContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestMethodFinished
		{
			AssemblyUniqueID = ctxt.TestMethod.TestClass.TestCollection.TestAssembly.UniqueID,
			ExecutionTime = summary.Time,
			TestClassUniqueID = ctxt.TestMethod.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestMethod.TestClass.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.TestMethod.UniqueID,
			TestsFailed = summary.Failed,
			TestsNotRun = summary.NotRun,
			TestsSkipped = summary.Skipped,
			TestsTotal = summary.Total,
		}));
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestMethodStarting(XunitTestMethodRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		return new(ctxt.MessageBus.QueueMessage(new TestMethodStarting
		{
			AssemblyUniqueID = ctxt.TestMethod.TestClass.TestCollection.TestAssembly.UniqueID,
			MethodName = Guard.ArgumentNotNull(ctxt).TestMethod.MethodName,
			TestClassUniqueID = ctxt.TestMethod.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestMethod.TestClass.TestCollection.UniqueID,
			TestMethodUniqueID = ctxt.TestMethod.UniqueID,
			Traits = ctxt.TestMethod.Traits,
		}));
	}

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestCaseAsync(
		XunitTestMethodRunnerContext ctxt,
		IXunitTestCase testCase,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(testCase);

		return
			exception is not null
				? new(XunitRunnerHelper.FailTestCases(ctxt.MessageBus, ctxt.CancellationTokenSource, [testCase], exception))
				: testCase.RunAsync(
					ctxt.ExplicitOption,
					ctxt.MessageBus,
					ctxt.ConstructorArguments,
					ctxt.Aggregator,
					ctxt.CancellationTokenSource
				);
	}
}
