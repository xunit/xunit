using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// An implementation of <see cref="TestCaseRunner{TContext, TTestCase}"/> to support <see cref="ExecutionErrorTestCase"/>.
/// </summary>
public class ExecutionErrorTestCaseRunner : TestCaseRunner<TestCaseRunnerContext<ExecutionErrorTestCase>, ExecutionErrorTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ExecutionErrorTestCaseRunner"/> class.
	/// </summary>
	protected ExecutionErrorTestCaseRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="ExecutionErrorTestCaseRunner"/> class.
	/// </summary>
	public static ExecutionErrorTestCaseRunner Instance { get; } = new();

	/// <summary>
	/// Runs the test case.
	/// </summary>
	/// <param name="testCase">The test case that this invocation belongs to.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <returns>Returns summary information about the test that was run.</returns>
	public ValueTask<RunSummary> RunAsync(
		ExecutionErrorTestCase testCase,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource) =>
			RunAsync(new(testCase, messageBus, aggregator, cancellationTokenSource));

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestsAsync(TestCaseRunnerContext<ExecutionErrorTestCase> ctxt)
	{
		// Use -1 for the index here so we don't collide with any legitimate test case IDs that might've been used
		var test = new XunitTest(ctxt.TestCase, ctxt.TestCase.TestCaseDisplayName, testIndex: -1);
		var summary = new RunSummary { Total = 1 };

		var testAssemblyUniqueID = ctxt.TestCase.TestMethod.TestClass.TestCollection.TestAssembly.UniqueID;
		var testCaseUniqueID = ctxt.TestCase.UniqueID;
		var testClassUniqueID = ctxt.TestCase.TestMethod.TestClass.UniqueID;
		var testCollectionUniqueID = ctxt.TestCase.TestMethod.TestClass.TestCollection.UniqueID;
		var testMethodUniqueID = ctxt.TestCase.TestMethod.UniqueID;

		var testStarting = new _TestStarting
		{
			AssemblyUniqueID = testAssemblyUniqueID,
			TestCaseUniqueID = testCaseUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestDisplayName = test.DisplayName,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = test.UniqueID
		};

		if (!ctxt.MessageBus.QueueMessage(testStarting))
			ctxt.CancellationTokenSource.Cancel();
		else
		{
			summary.Failed = 1;

			var testFailed = new _TestFailed
			{
				AssemblyUniqueID = testAssemblyUniqueID,
				ExceptionParentIndices = new[] { -1 },
				ExceptionTypes = new[] { typeof(InvalidOperationException).FullName },
				ExecutionTime = 0m,
				Messages = new[] { ctxt.TestCase.ErrorMessage },
				StackTraces = new[] { "" },
				Output = "",
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = test.UniqueID
			};

			if (!ctxt.MessageBus.QueueMessage(testFailed))
				ctxt.CancellationTokenSource.Cancel();

			var testFinished = new _TestFinished
			{
				AssemblyUniqueID = testAssemblyUniqueID,
				ExecutionTime = 0m,
				Output = "",
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = test.UniqueID
			};

			if (!ctxt.MessageBus.QueueMessage(testFinished))
				ctxt.CancellationTokenSource.Cancel();
		}

		return new(summary);
	}
}
