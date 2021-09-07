﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// An implementation of <see cref="TestCaseRunner{TTestCase}"/> to support <see cref="ExecutionErrorTestCase"/>.
	/// </summary>
	public class ExecutionErrorTestCaseRunner : TestCaseRunner<ExecutionErrorTestCase>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ExecutionErrorTestCaseRunner"/> class.
		/// </summary>
		/// <param name="testCase">The test case that the lambda represents.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		public ExecutionErrorTestCaseRunner(
			ExecutionErrorTestCase testCase,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
				: base(testCase, messageBus, aggregator, cancellationTokenSource)
		{ }

		/// <inheritdoc/>
		protected override Task<RunSummary> RunTestAsync()
		{
			// Use -1 for the index here so we don't collide with any legitimate test case IDs that might've been used
			var test = new XunitTest(TestCase, TestCase.TestCaseDisplayName, testIndex: -1);
			var summary = new RunSummary { Total = 1 };

			var testAssemblyUniqueID = TestCase.TestMethod.TestClass.TestCollection.TestAssembly.UniqueID;
			var testCaseUniqueID = TestCase.UniqueID;
			var testClassUniqueID = TestCase.TestMethod.TestClass.UniqueID;
			var testCollectionUniqueID = TestCase.TestMethod.TestClass.TestCollection.UniqueID;
			var testMethodUniqueID = TestCase.TestMethod.UniqueID;

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

			if (!MessageBus.QueueMessage(testStarting))
				CancellationTokenSource.Cancel();
			else
			{
				summary.Failed = 1;

				var testFailed = new _TestFailed
				{
					AssemblyUniqueID = testAssemblyUniqueID,
					ExceptionParentIndices = new[] { -1 },
					ExceptionTypes = new[] { typeof(InvalidOperationException).FullName },
					ExecutionTime = 0m,
					Messages = new[] { TestCase.ErrorMessage },
					StackTraces = new[] { "" },
					Output = "",
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestUniqueID = test.UniqueID
				};

				if (!MessageBus.QueueMessage(testFailed))
					CancellationTokenSource.Cancel();

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

				if (!MessageBus.QueueMessage(testFinished))
					CancellationTokenSource.Cancel();
			}

			return Task.FromResult(summary);
		}
	}
}
