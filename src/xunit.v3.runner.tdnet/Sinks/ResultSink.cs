using System;
using System.Threading;
using System.Threading.Tasks;
using TestDriven.Framework;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.v3;

namespace Xunit.Runner.TdNet
{
	public class ResultSink : TestMessageSink
	{
		readonly MessageMetadataCache metadataCache = new MessageMetadataCache();
		readonly int totalTests;

		public ResultSink(ITestListener listener, int totalTests)
		{
			this.totalTests = totalTests;
			TestListener = listener;
			TestRunState = TestRunState.NoTests;

			Execution.TestFailedEvent += HandleTestFailed;
			Execution.TestFinishedEvent +=
				args => metadataCache.TryRemove(args.Message);
			Execution.TestPassedEvent += HandleTestPassed;
			Execution.TestSkippedEvent += HandleTestSkipped;
			Execution.TestStartingEvent +=
				args => metadataCache.Set(args.Message);

			Diagnostics.ErrorMessageEvent +=
				args => ReportError("Fatal Error", args.Message);

			Execution.TestAssemblyCleanupFailureEvent +=
				args => ReportError($"Test Assembly Cleanup Failure ({metadataCache.TryGetAssemblyMetadata(args.Message)?.AssemblyPath ?? "<unknown test assembly>"})", args.Message);
			Execution.TestAssemblyFinishedEvent +=
				args => metadataCache.TryRemove(args.Message);
			Execution.TestAssemblyStartingEvent +=
				args => metadataCache.Set(args.Message);

			Execution.TestCaseCleanupFailureEvent +=
				args => ReportError($"Test Case Cleanup Failure ({metadataCache.TryGetTestCaseMetadata(args.Message)?.TestCaseDisplayName ?? "<unknown test case>"})", args.Message);
			Execution.TestCaseFinishedEvent +=
				args => metadataCache.TryRemove(args.Message);
			Execution.TestCaseStartingEvent +=
				args => metadataCache.Set(args.Message);

			Execution.TestClassCleanupFailureEvent +=
				args => ReportError($"Test Class Cleanup Failure ({metadataCache.TryGetClassMetadata(args.Message)?.TestClass ?? "<unknown test class>"})", args.Message);
			Execution.TestClassFinishedEvent +=
				args => metadataCache.TryRemove(args.Message);
			Execution.TestClassStartingEvent +=
				args => metadataCache.Set(args.Message);

			Execution.TestCollectionCleanupFailureEvent +=
				args => ReportError($"Test Collection Cleanup Failure ({metadataCache.TryGetCollectionMetadata(args.Message)?.TestCollectionDisplayName ?? "<unknown test collection>"})", args.Message);
			Execution.TestCollectionFinishedEvent +=
				args => metadataCache.TryRemove(args.Message);
			Execution.TestCollectionStartingEvent +=
				args => metadataCache.Set(args.Message);

			Execution.TestMethodCleanupFailureEvent +=
				args => ReportError($"Test Method Cleanup Failure ({metadataCache.TryGetMethodMetadata(args.Message)?.TestMethod ?? "<unknown test method>"})", args.Message);
			Execution.TestMethodFinishedEvent +=
				args => metadataCache.TryRemove(args.Message);
			Execution.TestMethodStartingEvent +=
				args => metadataCache.Set(args.Message);

			Execution.TestCleanupFailureEvent +=
				args => ReportError($"Test Cleanup Failure ({args.Message.Test.DisplayName})", args.Message);

			Execution.TestAssemblyFinishedEvent += args => Finished.Set();
		}

		public ManualResetEvent Finished { get; } = new ManualResetEvent(initialState: false);

		public ITestListener TestListener { get; }

		public TestRunState TestRunState { get; set; }

		void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
		{
			TestRunState = TestRunState.Failure;

			var testFailed = args.Message;
			var testResult = testFailed.ToTdNetTestResult(TestState.Failed, totalTests);
			testResult.Message = ExceptionUtility.CombineMessages(testFailed);
			testResult.StackTrace = ExceptionUtility.CombineStackTraces(testFailed);

			TestListener.TestFinished(testResult);

			WriteOutput(testFailed.Test.DisplayName, testFailed.Output);
		}

		void HandleTestPassed(MessageHandlerArgs<_TestPassed> args)
		{
			if (TestRunState == TestRunState.NoTests)
				TestRunState = TestRunState.Success;

			var testPassed = args.Message;
			var testResult = ToTdNetTestResult(testPassed, TestState.Passed, totalTests);

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

			TestListener.TestFinished(testResult);
		}

		void ReportError(string messageType, IFailureInformation failureInfo)
		{
			TestRunState = TestRunState.Failure;

			var testResult = new TestResult
			{
				Name = $"*** {messageType} ***",
				State = TestState.Failed,
				TimeSpan = TimeSpan.Zero,
				TotalTests = 1,
				Message = ExceptionUtility.CombineMessages(failureInfo),
				StackTrace = ExceptionUtility.CombineStackTraces(failureInfo)
			};

			TestListener.TestFinished(testResult);
		}

		TestResult ToTdNetTestResult(
			_TestResultMessage testResult,
			TestState testState,
			int totalTests)
		{
			var testClassMetadata = Guard.NotNull($"Cannot get test class metadata for ID {testResult.TestClassUniqueID}", metadataCache.TryGetClassMetadata(testResult));
			var testClass = Type.GetType(testClassMetadata.TestClass);
			var testMethodMetadata = Guard.NotNull($"Cannot get test method metadata for ID {testResult.TestMethodUniqueID}", metadataCache.TryGetMethodMetadata(testResult));
			var testMethod = testClass != null ? testClass.GetMethod(testMethodMetadata.TestMethod) : null;
			var testMetadata = Guard.NotNull($"Cannot get test metadata for ID {testResult.TestUniqueID}", metadataCache.TryGetTestMetadata(testResult));

			return new TestResult
			{
				FixtureType = testClass,
				Method = testMethod,
				Name = testMetadata.TestDisplayName,
				State = testState,
				TimeSpan = new TimeSpan((long)(10000.0M * testResult.ExecutionTime)),
				TotalTests = totalTests,
			};
		}

		void WriteOutput(string name, string output)
		{
			if (string.IsNullOrWhiteSpace(output))
				return;

			TestListener.WriteLine($"Output from {name}:", Category.Output);
			foreach (var line in output.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None))
				TestListener.WriteLine($"  {line}", Category.Output);
		}

		public override async ValueTask DisposeAsync()
		{
			await base.DisposeAsync();
			Finished.Dispose();
		}
	}
}
