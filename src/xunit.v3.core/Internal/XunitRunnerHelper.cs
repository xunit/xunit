using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Helper functions for xUnit.net v3 runners.
/// </summary>
public static class XunitRunnerHelper
{
	/// <summary>
	/// Fail a set of test cases with the given message.
	/// </summary>
	/// <param name="messageBus">The message bus to send the messages to</param>
	/// <param name="cancellationTokenSource">The cancellation token source to cancel if requested</param>
	/// <param name="testCases">The test cases to fail</param>
	/// <param name="messageFormat">A message template where <c>{0}</c> will be replaced with the
	/// display name of the test case during failure processing</param>
	/// <param name="sendTestCollectionMessages">Set to <c>true</c> to send <see cref="ITestCollectionStarting"/>
	/// and <see cref="ITestCollectionFinished"/> messages; set to <c>false</c> to skip</param>
	/// <param name="sendTestClassMessages">Set to <c>true</c> to send <see cref="ITestClassStarting"/>
	/// and <see cref="ITestClassFinished"/> messages; set to <c>false</c> to skip</param>
	/// <param name="sendTestMethodMessages">Set to <c>true</c> to send <see cref="ITestMethodStarting"/>
	/// and <see cref="ITestMethodFinished"/> messages; set to <c>false</c> to skip</param>
	/// <param name="sendTestCaseMessages">Set to <c>true</c> to send <see cref="ITestCaseStarting"/>
	/// and <see cref="ITestCaseFinished"/> messages; set to <c>false</c> to skip</param>
	/// <param name="sendTestMessages">Set to <c>true</c> to send <see cref="ITestStarting"/>
	/// and <see cref="ITestFinished"/> messages; set to <c>false</c> to skip</param>
	public static RunSummary FailTestCases(
		IMessageBus messageBus,
		CancellationTokenSource cancellationTokenSource,
		IReadOnlyCollection<ITestCase> testCases,
		string messageFormat,
		bool sendTestCollectionMessages = false,
		bool sendTestClassMessages = false,
		bool sendTestMethodMessages = false,
		bool sendTestCaseMessages = true,
		bool sendTestMessages = true)
	{
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(cancellationTokenSource);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageFormat);

		var result = new RunSummary();

		foreach (var testCase in testCases)
		{
			result.Total++;
			result.Failed++;

			FailTestCase(
				messageBus, cancellationTokenSource, testCase, -result.Total,
				FailureCause.Exception, [-1], [string.Format(CultureInfo.CurrentCulture, messageFormat, testCase.TestCaseDisplayName)], [typeof(TestPipelineException).SafeName()], [null],
				sendTestCollectionMessages, sendTestClassMessages, sendTestMethodMessages, sendTestCaseMessages, sendTestMessages
			);
		}

		return result;
	}

	/// <summary>
	/// Fail a set of test cases with the given exception.
	/// </summary>
	/// <param name="messageBus">The message bus to send the messages to</param>
	/// <param name="cancellationTokenSource">The cancellation token source to cancel if requested</param>
	/// <param name="testCases">The test cases to fail</param>
	/// <param name="exception">The exception to fail the test cases with</param>
	/// <param name="sendTestCollectionMessages">Set to <c>true</c> to send <see cref="ITestCollectionStarting"/>
	/// and <see cref="ITestCollectionFinished"/> messages; set to <c>false</c> to skip</param>
	/// <param name="sendTestClassMessages">Set to <c>true</c> to send <see cref="ITestClassStarting"/>
	/// and <see cref="ITestClassFinished"/> messages; set to <c>false</c> to skip</param>
	/// <param name="sendTestMethodMessages">Set to <c>true</c> to send <see cref="ITestMethodStarting"/>
	/// and <see cref="ITestMethodFinished"/> messages; set to <c>false</c> to skip</param>
	/// <param name="sendTestCaseMessages">Set to <c>true</c> to send <see cref="ITestCaseStarting"/>
	/// and <see cref="ITestCaseFinished"/> messages; set to <c>false</c> to skip</param>
	/// <param name="sendTestMessages">Set to <c>true</c> to send <see cref="ITestStarting"/>
	/// and <see cref="ITestFinished"/> messages; set to <c>false</c> to skip</param>
	public static RunSummary FailTestCases(
		IMessageBus messageBus,
		CancellationTokenSource cancellationTokenSource,
		IReadOnlyCollection<ITestCase> testCases,
		Exception exception,
		bool sendTestCollectionMessages = false,
		bool sendTestClassMessages = false,
		bool sendTestMethodMessages = false,
		bool sendTestCaseMessages = true,
		bool sendTestMessages = true)
	{
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(cancellationTokenSource);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(exception);

		var result = new RunSummary();

		foreach (var testCase in testCases)
		{
			result.Total++;
			result.Failed++;

			var (types, messages, stackTraces, indices, cause) = ExceptionUtility.ExtractMetadata(exception);

			FailTestCase(
				messageBus, cancellationTokenSource, testCase, -result.Total,
				cause, indices, messages, types, stackTraces,
				sendTestCollectionMessages, sendTestClassMessages, sendTestMethodMessages, sendTestCaseMessages, sendTestMessages
			);
		}

		return result;
	}

	/// <summary>
	/// Skips a set of test cases with the given skip reason.
	/// </summary>
	/// <param name="messageBus">The message bus to send the messages to</param>
	/// <param name="cancellationTokenSource">The cancellation token source to cancel if requested</param>
	/// <param name="testCases">The test cases to fail</param>
	/// <param name="skipReason">The skip reason</param>
	/// <param name="sendTestCollectionMessages">Set to <c>true</c> to send <see cref="ITestCollectionStarting"/>
	/// and <see cref="ITestCollectionFinished"/> messages; set to <c>false</c> to skip</param>
	/// <param name="sendTestClassMessages">Set to <c>true</c> to send <see cref="ITestClassStarting"/>
	/// and <see cref="ITestClassFinished"/> messages; set to <c>false</c> to skip</param>
	/// <param name="sendTestMethodMessages">Set to <c>true</c> to send <see cref="ITestMethodStarting"/>
	/// and <see cref="ITestMethodFinished"/> messages; set to <c>false</c> to skip</param>
	/// <param name="sendTestCaseMessages">Set to <c>true</c> to send <see cref="ITestCaseStarting"/>
	/// and <see cref="ITestCaseFinished"/> messages; set to <c>false</c> to skip</param>
	/// <param name="sendTestMessages">Set to <c>true</c> to send <see cref="ITestStarting"/>
	/// and <see cref="ITestFinished"/> messages; set to <c>false</c> to skip</param>
	public static RunSummary SkipTestCases(
		IMessageBus messageBus,
		CancellationTokenSource cancellationTokenSource,
		IReadOnlyCollection<ITestCase> testCases,
		string skipReason,
		bool sendTestCollectionMessages = false,
		bool sendTestClassMessages = false,
		bool sendTestMethodMessages = false,
		bool sendTestCaseMessages = true,
		bool sendTestMessages = true)
	{
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(cancellationTokenSource);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(skipReason);

		var result = new RunSummary();

		foreach (var testCase in testCases)
		{
			result.Total++;
			result.Failed++;

			SkipTestCase(
				messageBus, cancellationTokenSource, testCase, -result.Total, skipReason,
				sendTestCollectionMessages, sendTestClassMessages, sendTestMethodMessages, sendTestCaseMessages, sendTestMessages
			);
		}

		return result;
	}

	static void ExecuteTestCase(
		IMessageBus messageBus,
		CancellationTokenSource cancellationTokenSource,
		ITestCase testCase,
		int testIndex,
		TestResultMessage testResultMessage,
		bool sendTestCollectionMessages,
		bool sendTestClassMessages,
		bool sendTestMethodMessages,
		bool sendTestCaseMessages,
		bool sendTestMessages)
	{
		var assemblyUniqueID = testCase.TestCollection.TestAssembly.UniqueID;
		var testCollectionUniqueID = testCase.TestCollection.UniqueID;
		var testCaseUniqueID = testCase.UniqueID;
		var testClassUniqueID = testCase.TestClass?.UniqueID;
		var testMethodUniqueID = testCase.TestMethod?.UniqueID;
		var testUniqueID = UniqueIDGenerator.ForTest(testCaseUniqueID, testIndex);
		var now = DateTimeOffset.UtcNow;

		if (sendTestCollectionMessages)
			if (!messageBus.QueueMessage(new TestCollectionStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCollectionClassName = testCase.TestCollection.TestCollectionClassName,
				TestCollectionDisplayName = testCase.TestCollection.TestCollectionDisplayName,
				TestCollectionUniqueID = testCollectionUniqueID,
				Traits = testCase.TestCollection.Traits,
			}))
				cancellationTokenSource.Cancel();

		if (sendTestClassMessages && testCase.TestClass is not null)
			if (!messageBus.QueueMessage(new TestClassStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestClassName = testCase.TestClass.TestClassName,
				TestClassNamespace = testCase.TestClass.TestClassNamespace,
				TestClassSimpleName = testCase.TestClass.TestClassSimpleName,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				Traits = testCase.TestClass.Traits,
			}))
				cancellationTokenSource.Cancel();

		if (sendTestMethodMessages && testCase.TestMethod is not null)
			if (!messageBus.QueueMessage(new TestMethodStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				MethodName = testCase.TestMethod.MethodName,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				Traits = testCase.TestMethod.Traits,
			}))
				cancellationTokenSource.Cancel();

		if (sendTestCaseMessages)
			if (!messageBus.QueueMessage(new TestCaseStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				Explicit = testCase.Explicit,
				SkipReason = testCase.SkipReason,
				SourceFilePath = testCase.SourceFilePath,
				SourceLineNumber = testCase.SourceLineNumber,
				TestCaseDisplayName = testCase.TestCaseDisplayName,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassMetadataToken = testCase.TestClassMetadataToken,
				TestClassName = testCase.TestClassName,
				TestClassNamespace = testCase.TestClassNamespace,
				TestClassSimpleName = testCase.TestClassSimpleName,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodMetadataToken = testCase.TestMethodMetadataToken,
				TestMethodName = testCase.TestMethod?.MethodName,
				TestMethodParameterTypesVSTest = testCase.TestMethodParameterTypesVSTest,
				TestMethodReturnTypeVSTest = testCase.TestMethodReturnTypeVSTest,
				TestMethodUniqueID = testMethodUniqueID,
				Traits = testCase.Traits,
			}))
				cancellationTokenSource.Cancel();

		if (sendTestMessages)
			if (!messageBus.QueueMessage(new TestStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				Explicit = false,
				StartTime = now,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestDisplayName = testCase.TestCaseDisplayName,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
				Timeout = 0,
				Traits = testCase.Traits,
			}))
				cancellationTokenSource.Cancel();

		testResultMessage.FinishTime = now;

		if (!messageBus.QueueMessage(testResultMessage))
			cancellationTokenSource.Cancel();

		if (sendTestMessages)
			if (!messageBus.QueueMessage(new TestFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				Attachments = TestFinished.EmptyAttachments,
				ExecutionTime = 0m,
				FinishTime = now,
				Output = string.Empty,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
				Warnings = null,
			}))
				cancellationTokenSource.Cancel();

		if (sendTestCaseMessages)
			if (!messageBus.QueueMessage(new TestCaseFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = 0m,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestsFailed = 1,
				TestsNotRun = 0,
				TestsSkipped = 0,
				TestsTotal = 1,
			}))
				cancellationTokenSource.Cancel();

		if (sendTestMethodMessages)
			if (!messageBus.QueueMessage(new TestMethodFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = 0m,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestsFailed = 1,
				TestsNotRun = 0,
				TestsSkipped = 0,
				TestsTotal = 1,
			}))
				cancellationTokenSource.Cancel();

		if (sendTestClassMessages)
			if (!messageBus.QueueMessage(new TestClassFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = 0m,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestsFailed = 1,
				TestsNotRun = 0,
				TestsSkipped = 0,
				TestsTotal = 1,
			}))
				cancellationTokenSource.Cancel();

		if (sendTestCollectionMessages)
			if (!messageBus.QueueMessage(new TestCollectionFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = 0m,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestsFailed = 1,
				TestsNotRun = 0,
				TestsSkipped = 0,
				TestsTotal = 1,
			}))
				cancellationTokenSource.Cancel();
	}

	static void FailTestCase(
		IMessageBus messageBus,
		CancellationTokenSource cancellationTokenSource,
		ITestCase testCase,
		int testIndex,
		FailureCause cause,
		int[] parentIndices,
		string[] messages,
		string?[] types,
		string?[] stackTraces,
		bool sendTestCollectionMessages,
		bool sendTestClassMessages,
		bool sendTestMethodMessages,
		bool sendTestCaseMessages,
		bool sendTestMessages) =>
			ExecuteTestCase(
				messageBus,
				cancellationTokenSource,
				testCase,
				testIndex,
				new TestFailed
				{
					AssemblyUniqueID = testCase.TestCollection.TestAssembly.UniqueID,
					Cause = cause,
					ExceptionParentIndices = parentIndices,
					ExceptionTypes = types,
					ExecutionTime = 0m,
					FinishTime = DateTimeOffset.UtcNow,
					Messages = messages,
					Output = string.Empty,
					StackTraces = stackTraces,
					TestCaseUniqueID = testCase.UniqueID,
					TestClassUniqueID = testCase.TestClass?.UniqueID,
					TestCollectionUniqueID = testCase.TestCollection.UniqueID,
					TestMethodUniqueID = testCase.TestMethod?.UniqueID,
					TestUniqueID = UniqueIDGenerator.ForTest(testCase.UniqueID, testIndex),
					Warnings = null,
				},
				sendTestCollectionMessages,
				sendTestClassMessages,
				sendTestMethodMessages,
				sendTestCaseMessages,
				sendTestMessages
			);

	static void SkipTestCase(
		IMessageBus messageBus,
		CancellationTokenSource cancellationTokenSource,
		ITestCase testCase,
		int testIndex,
		string skipReason,
		bool sendTestCollectionMessages,
		bool sendTestClassMessages,
		bool sendTestMethodMessages,
		bool sendTestCaseMessages,
		bool sendTestMessages) =>
			ExecuteTestCase(
				messageBus,
				cancellationTokenSource,
				testCase,
				testIndex,
				new TestSkipped
				{
					AssemblyUniqueID = testCase.TestCollection.TestAssembly.UniqueID,
					ExecutionTime = 0m,
					FinishTime = DateTimeOffset.UtcNow,
					Output = string.Empty,
					Reason = skipReason,
					TestCaseUniqueID = testCase.UniqueID,
					TestClassUniqueID = testCase.TestClass?.UniqueID,
					TestCollectionUniqueID = testCase.TestCollection.UniqueID,
					TestMethodUniqueID = testCase.TestMethod?.UniqueID,
					TestUniqueID = UniqueIDGenerator.ForTest(testCase.UniqueID, testIndex),
					Warnings = null,
				},
				sendTestCollectionMessages,
				sendTestClassMessages,
				sendTestMethodMessages,
				sendTestCaseMessages,
				sendTestMessages
			);
}
