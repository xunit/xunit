using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// An implementation of <see cref="TestCaseRunner{TTestCase}"/> to support <see cref="ExecutionErrorTestCase"/>.
	/// </summary>
	public class ExecutionErrorTestCaseRunner : TestCaseRunner<ExecutionErrorTestCase>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ExecutionErrorTestCaseRunner"/> class.
		/// </summary>
		/// <param name="testAssemblyUniqueID">The test assembly unique ID.</param>
		/// <param name="testCollectionUniqueID">The test collection unique ID.</param>
		/// <param name="testClassUniqueID">The test class unique ID.</param>
		/// <param name="testMethodUniqueID">The test method unique ID.</param>
		/// <param name="testCase">The test case that the lambda represents.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		public ExecutionErrorTestCaseRunner(
			string testAssemblyUniqueID,
			string testCollectionUniqueID,
			string? testClassUniqueID,
			string? testMethodUniqueID,
			ExecutionErrorTestCase testCase,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
				: base(testAssemblyUniqueID, testCollectionUniqueID, testClassUniqueID, testMethodUniqueID, testCase, messageBus, aggregator, cancellationTokenSource)
		{ }

		/// <inheritdoc/>
		protected override Task<RunSummary> RunTestAsync()
		{
			// Use -1 for the index here so we don't collide with any legitimate test case IDs that might've been used
			var test = new XunitTest(TestCase, TestCase.DisplayName, testIndex: -1);
			var summary = new RunSummary { Total = 1 };

			var testStarting = new _TestStarting
			{
				AssemblyUniqueID = TestAssemblyUniqueID,
				TestCaseUniqueID = TestCase.UniqueID,
				TestClassUniqueID = TestClassUniqueID,
				TestCollectionUniqueID = TestCollectionUniqueID,
				TestDisplayName = test.DisplayName,
				TestMethodUniqueID = TestMethodUniqueID,
				TestUniqueID = test.UniqueID
			};

			if (!MessageBus.QueueMessage(testStarting))
				CancellationTokenSource.Cancel();
			else
			{
				summary.Failed = 1;

				var testFailed = new _TestFailed
				{
					AssemblyUniqueID = TestAssemblyUniqueID,
					ExceptionParentIndices = new[] { -1 },
					ExceptionTypes = new[] { typeof(InvalidOperationException).FullName },
					ExecutionTime = 0m,
					Messages = new[] { TestCase.ErrorMessage },
					StackTraces = new[] { "" },
					Output = "",
					TestCaseUniqueID = TestCase.UniqueID,
					TestClassUniqueID = TestClassUniqueID,
					TestCollectionUniqueID = TestCollectionUniqueID,
					TestMethodUniqueID = TestMethodUniqueID,
					TestUniqueID = test.UniqueID
				};

				if (!MessageBus.QueueMessage(testFailed))
					CancellationTokenSource.Cancel();

				var testFinished = new _TestFinished
				{
					AssemblyUniqueID = TestAssemblyUniqueID,
					ExecutionTime = 0m,
					Output = "",
					TestCaseUniqueID = TestCase.UniqueID,
					TestClassUniqueID = TestClassUniqueID,
					TestCollectionUniqueID = TestCollectionUniqueID,
					TestMethodUniqueID = TestMethodUniqueID,
					TestUniqueID = test.UniqueID
				};

				if (!MessageBus.QueueMessage(testFinished))
					CancellationTokenSource.Cancel();
			}

			return Task.FromResult(summary);
		}
	}
}
