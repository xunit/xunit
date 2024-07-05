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
		Exception exception) =>
			new(ReportMessage(ctxt, new _TestMethodCleanupFailure(), exception: exception));

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestMethodFinished(
		XunitTestMethodRunnerContext ctxt,
		RunSummary summary) =>
			new(ReportMessage(ctxt, new _TestMethodFinished(), summary: summary));

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestMethodStarting(XunitTestMethodRunnerContext ctxt) =>
		new(ReportMessage(ctxt, new _TestMethodStarting
		{
			MethodName = Guard.ArgumentNotNull(ctxt).TestMethod.MethodName,
			Traits = ctxt.TestMethod.Traits,
		}));

	static bool ReportMessage(
		XunitTestMethodRunnerContext ctxt,
		_TestMethodMessage message,
		RunSummary summary = default,
		Exception? exception = null)
	{
		Guard.ArgumentNotNull(ctxt);

		message.AssemblyUniqueID = ctxt.TestMethod.TestClass.TestCollection.TestAssembly.UniqueID;
		message.TestClassUniqueID = ctxt.TestMethod.TestClass.UniqueID;
		message.TestCollectionUniqueID = ctxt.TestMethod.TestClass.TestCollection.UniqueID;
		message.TestMethodUniqueID = ctxt.TestMethod?.UniqueID;

		if (message is _IWritableExecutionSummaryMetadata summaryMessage)
		{
			summaryMessage.ExecutionTime = summary.Time;
			summaryMessage.TestsFailed = summary.Failed;
			summaryMessage.TestsNotRun = summary.NotRun;
			summaryMessage.TestsSkipped = summary.Skipped;
			summaryMessage.TestsTotal = summary.Total;
		}

		if (exception is not null && message is _IWritableErrorMetadata errorMessage)
		{
			var (types, messages, stackTraces, indices, _) = ExceptionUtility.ExtractMetadata(exception);

			errorMessage.ExceptionParentIndices = indices;
			errorMessage.ExceptionTypes = types;
			errorMessage.Messages = messages;
			errorMessage.StackTraces = stackTraces;
		}

		return ctxt.MessageBus.QueueMessage(message);
	}

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestCaseAsync(
		XunitTestMethodRunnerContext ctxt,
		IXunitTestCase testCase,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(testCase);

		if (exception is not null)
			return new(XunitRunnerHelper.FailTestCases(ctxt.MessageBus, ctxt.CancellationTokenSource, [testCase], exception));

		return testCase.RunAsync(
			ctxt.ExplicitOption,
			ctxt.MessageBus,
			ctxt.ConstructorArguments,
			ctxt.Aggregator,
			ctxt.CancellationTokenSource
		);
	}
}
