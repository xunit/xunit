using System;
using System.Threading;
using System.Threading.Tasks;
using TestDriven.Framework;
using Xunit.Abstractions;
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
			Execution.TestPassedEvent += HandleTestPassed;
			Execution.TestSkippedEvent += HandleTestSkipped;

			Diagnostics.ErrorMessageEvent +=
				args => ReportError("Fatal Error", args.Message);

			Execution.TestAssemblyCleanupFailureEvent +=
				args => ReportError($"Test Assembly Cleanup Failure ({args.Message.TestAssembly.Assembly.AssemblyPath})", args.Message);

			Execution.TestCaseCleanupFailureEvent +=
				args => ReportError($"Test Case Cleanup Failure ({args.Message.TestCase.DisplayName})", args.Message);

			Execution.TestClassCleanupFailureEvent +=
				args => ReportError($"Test Class Cleanup Failure ({args.Message.TestClass.Class.Name})", args.Message);

			Execution.TestCollectionCleanupFailureEvent +=
				args => ReportError($"Test Collection Cleanup Failure ({metadataCache.TryGet(args.Message)?.TestCollectionDisplayName ?? "<unknown test collection>"})", args.Message);
			Execution.TestCollectionFinishedEvent +=
				args => metadataCache.TryRemove(args.Message);
			Execution.TestCollectionStartingEvent +=
				args => metadataCache.Set(args.Message);

			Execution.TestMethodCleanupFailureEvent +=
				args => ReportError($"Test Method Cleanup Failure ({args.Message.TestMethod.Method.Name})", args.Message);

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

		void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
		{
			if (TestRunState == TestRunState.NoTests)
				TestRunState = TestRunState.Success;

			var testPassed = args.Message;
			var testResult = testPassed.ToTdNetTestResult(TestState.Passed, totalTests);

			TestListener.TestFinished(testResult);

			WriteOutput(testPassed.Test.DisplayName, testPassed.Output);
		}

		void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
		{
			if (TestRunState == TestRunState.NoTests)
				TestRunState = TestRunState.Success;

			var testSkipped = args.Message;
			var testResult = testSkipped.ToTdNetTestResult(TestState.Ignored, totalTests);
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
