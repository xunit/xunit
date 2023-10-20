using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using TestDriven.Framework;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.TdNet;

public class ResultSink : TestMessageSink
{
	readonly MessageMetadataCache metadataCache = new();
	readonly int totalTests;

	public ResultSink(
		ITestListener listener,
		object listenerLock,
		int totalTests)
	{
		this.totalTests = totalTests;
		TestListener = listener;
		TestListenerLock = listenerLock;
		TestRunState = TestRunState.NoTests;

		Execution.TestCleanupFailureEvent +=
			args => ReportError(args.Message, "Test Cleanup Failure ({0})", metadataCache.TryGetTestMetadata(args.Message)?.TestDisplayName ?? "<unknown test>");
		Execution.TestFailedEvent += HandleTestFailed;
		Execution.TestFinishedEvent +=
			args => metadataCache.TryRemove(args.Message);
		Execution.TestPassedEvent += HandleTestPassed;
		Execution.TestSkippedEvent += HandleTestSkipped;
		Execution.TestStartingEvent +=
			args => metadataCache.Set(args.Message);

		Diagnostics.ErrorMessageEvent +=
			args => ReportError(args.Message, "Fatal Error");

		Execution.TestAssemblyCleanupFailureEvent +=
			args => ReportError(args.Message, "Test Assembly Cleanup Failure ({0})", metadataCache.TryGetAssemblyMetadata(args.Message)?.AssemblyPath ?? "<unknown test assembly>");
		Execution.TestAssemblyFinishedEvent +=
			args => metadataCache.TryRemove(args.Message);
		Execution.TestAssemblyStartingEvent +=
			args => metadataCache.Set(args.Message);

		Execution.TestCaseCleanupFailureEvent +=
			args => ReportError(args.Message, "Test Case Cleanup Failure ({0})", metadataCache.TryGetTestCaseMetadata(args.Message)?.TestCaseDisplayName ?? "<unknown test case>");
		Execution.TestCaseFinishedEvent +=
			args => metadataCache.TryRemove(args.Message);
		Execution.TestCaseStartingEvent +=
			args => metadataCache.Set(args.Message);

		Execution.TestClassCleanupFailureEvent +=
			args => ReportError(args.Message, "Test Class Cleanup Failure ({0})", metadataCache.TryGetClassMetadata(args.Message)?.TestClass ?? "<unknown test class>");
		Execution.TestClassFinishedEvent +=
			args => metadataCache.TryRemove(args.Message);
		Execution.TestClassStartingEvent +=
			args => metadataCache.Set(args.Message);

		Execution.TestCollectionCleanupFailureEvent +=
			args => ReportError(args.Message, "Test Collection Cleanup Failure ({0})", metadataCache.TryGetCollectionMetadata(args.Message)?.TestCollectionDisplayName ?? "<unknown test collection>");
		Execution.TestCollectionFinishedEvent +=
			args => metadataCache.TryRemove(args.Message);
		Execution.TestCollectionStartingEvent +=
			args => metadataCache.Set(args.Message);

		Execution.TestMethodCleanupFailureEvent +=
			args => ReportError(args.Message, "Test Method Cleanup Failure ({0})", metadataCache.TryGetMethodMetadata(args.Message)?.TestMethod ?? "<unknown test method>");
		Execution.TestMethodFinishedEvent +=
			args => metadataCache.TryRemove(args.Message);
		Execution.TestMethodStartingEvent +=
			args => metadataCache.Set(args.Message);

		Execution.TestAssemblyFinishedEvent +=
			_ => Finished.Set();
	}

	public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

	public ITestListener TestListener { get; }

	public object TestListenerLock { get; }

	public TestRunState TestRunState { get; set; }

	void HandleTestFailed(MessageHandlerArgs<_TestFailed> args)
	{
		TestRunState = TestRunState.Failure;

		var testFailed = args.Message;
		var testResult = ToTdNetTestResult(testFailed, TestState.Failed, totalTests);
		testResult.Message = ExceptionUtility.CombineMessages(testFailed);
		testResult.StackTrace = ExceptionUtility.CombineStackTraces(testFailed);

		lock (TestListenerLock)
			TestListener.TestFinished(testResult);

		WriteOutput(testResult.Name, testFailed.Output);
	}

	void HandleTestPassed(MessageHandlerArgs<_TestPassed> args)
	{
		if (TestRunState == TestRunState.NoTests)
			TestRunState = TestRunState.Success;

		var testPassed = args.Message;
		var testResult = ToTdNetTestResult(testPassed, TestState.Passed, totalTests);

		lock (TestListenerLock)
			TestListener.TestFinished(testResult);

		WriteOutput(testResult.Name, testPassed.Output);
	}

	void HandleTestSkipped(MessageHandlerArgs<_TestSkipped> args)
	{
		if (TestRunState == TestRunState.NoTests)
			TestRunState = TestRunState.Success;

		var testSkipped = args.Message;
		var testResult = ToTdNetTestResult(testSkipped, TestState.Ignored, totalTests);
		testResult.Message = testSkipped.Reason;

		lock (TestListenerLock)
			TestListener.TestFinished(testResult);
	}

	void ReportError(
		_IErrorMetadata errorMetadata,
		string messageType)
	{
		TestRunState = TestRunState.Failure;

		var testResult = new TestResult
		{
			Name = string.Format(CultureInfo.CurrentCulture, "*** {0} ***", messageType),
			State = TestState.Failed,
			TimeSpan = TimeSpan.Zero,
			TotalTests = 1,
			Message = ExceptionUtility.CombineMessages(errorMetadata),
			StackTrace = ExceptionUtility.CombineStackTraces(errorMetadata)
		};

		lock (TestListenerLock)
			TestListener.TestFinished(testResult);
	}

	void ReportError(
		_IErrorMetadata errorMetadata,
		string messageTypeFormat,
		params object?[] args) =>
			ReportError(errorMetadata, string.Format(CultureInfo.CurrentCulture, messageTypeFormat, args));

	TestResult ToTdNetTestResult(
		_TestResultMessage testResult,
		TestState testState,
		int testCount)
	{
		var testClassMetadata = Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Cannot get test class metadata for ID {0}", testResult.TestClassUniqueID), metadataCache.TryGetClassMetadata(testResult));
		var testClass = Type.GetType(testClassMetadata.TestClass);
		var testMethodMetadata = Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Cannot get test method metadata for ID {0}", testResult.TestMethodUniqueID), metadataCache.TryGetMethodMetadata(testResult));
		var testMethod = testClass?.GetMethod(testMethodMetadata.TestMethod);
		var testMetadata = Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Cannot get test metadata for ID {0}", testResult.TestUniqueID), metadataCache.TryGetTestMetadata(testResult));

		return new TestResult
		{
			FixtureType = testClass,
			Method = testMethod,
			Name = testMetadata.TestDisplayName,
			State = testState,
			TimeSpan = new TimeSpan((long)(10000.0M * testResult.ExecutionTime)),
			TotalTests = testCount,
		};
	}

	void WriteOutput(
		string name,
		string output)
	{
		if (string.IsNullOrWhiteSpace(output))
			return;

		lock (TestListenerLock)
		{
			TestListener.WriteLine(string.Format(CultureInfo.CurrentCulture, "Output from {0}:", name), Category.Output);

			foreach (var line in output.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
				TestListener.WriteLine(string.Format(CultureInfo.CurrentCulture, "  {0}", line), Category.Output);
		}
	}

	public override async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		await base.DisposeAsync();
		Finished.Dispose();
	}
}
