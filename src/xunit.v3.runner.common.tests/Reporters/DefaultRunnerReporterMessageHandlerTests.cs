using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class DefaultRunnerReporterMessageHandlerTests
{
	public class FailureMessages
	{
		internal static readonly string assemblyID = "assembly-id";
		internal static readonly string classID = "test-class-id";
		internal static readonly string collectionID = "test-collection-id";
		internal static readonly int[] exceptionParentIndices = new[] { -1 };
		internal static readonly string[] exceptionTypes = new[] { "ExceptionType" };
		internal static readonly string[] messages = new[] { $"This is my message \t{Environment.NewLine}Message Line 2" };
		internal static readonly string methodID = "test-method-id";
		internal static readonly string[] stackTraces = new[] { $"Line 1{Environment.NewLine}at SomeClass.SomeMethod() in SomeFolder\\SomeClass.cs:line 18{Environment.NewLine}Line 3" };
		internal static readonly string testCaseID = "test-case-id";
		readonly string testID = "test-id";

		[Fact]
		public void ErrorMessage()
		{
			var errorMessage = new _ErrorMessage
			{
				ExceptionParentIndices = exceptionParentIndices,
				ExceptionTypes = exceptionTypes,
				Messages = messages,
				StackTraces = stackTraces
			};
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(errorMessage);

			AssertFailureMessages(handler.Messages, "FATAL ERROR");
		}

		[Fact]
		public void TestAssemblyCleanupFailure()
		{
			var assemblyStarting = new _TestAssemblyStarting
			{
				AssemblyUniqueID = assemblyID,
				AssemblyPath = @"C:\Foo\bar.dll"
			};
			var assemblyCleanupFailure = new _TestAssemblyCleanupFailure
			{
				AssemblyUniqueID = assemblyID,
				ExceptionParentIndices = exceptionParentIndices,
				ExceptionTypes = exceptionTypes,
				Messages = messages,
				StackTraces = stackTraces
			};
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(assemblyStarting);
			handler.OnMessage(assemblyCleanupFailure);

			AssertFailureMessages(handler.Messages, @"Test Assembly Cleanup Failure (C:\Foo\bar.dll)");
		}

		[Fact]
		public void TestCaseCleanupFailure()
		{
			var caseStarting = new _TestCaseStarting
			{
				AssemblyUniqueID = assemblyID,
				TestCaseUniqueID = testCaseID,
				TestCaseDisplayName = "MyTestCase",
				TestClassUniqueID = classID,
				TestCollectionUniqueID = collectionID,
				TestMethodUniqueID = methodID
			};
			var caseCleanupFailure = new _TestCaseCleanupFailure
			{
				AssemblyUniqueID = assemblyID,
				ExceptionParentIndices = exceptionParentIndices,
				ExceptionTypes = exceptionTypes,
				Messages = messages,
				StackTraces = stackTraces,
				TestCaseUniqueID = testCaseID,
				TestCollectionUniqueID = collectionID,
				TestClassUniqueID = classID,
				TestMethodUniqueID = methodID
			};
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(caseStarting);
			handler.OnMessage(caseCleanupFailure);

			AssertFailureMessages(handler.Messages, "Test Case Cleanup Failure (MyTestCase)");
		}

		[Fact]
		public void TestClassCleanupFailure()
		{
			var classStarting = new _TestClassStarting
			{
				AssemblyUniqueID = assemblyID,
				TestClass = "MyType",
				TestClassUniqueID = classID,
				TestCollectionUniqueID = collectionID
			};
			var classCleanupFailure = new _TestClassCleanupFailure
			{
				AssemblyUniqueID = assemblyID,
				ExceptionParentIndices = exceptionParentIndices,
				ExceptionTypes = exceptionTypes,
				Messages = messages,
				StackTraces = stackTraces,
				TestCollectionUniqueID = collectionID,
				TestClassUniqueID = classID
			};
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(classStarting);
			handler.OnMessage(classCleanupFailure);

			AssertFailureMessages(handler.Messages, "Test Class Cleanup Failure (MyType)");
		}

		[Fact]
		public void TestCleanupFailure()
		{
			var testStarting = new _TestStarting
			{
				AssemblyUniqueID = assemblyID,
				TestCaseUniqueID = testCaseID,
				TestClassUniqueID = classID,
				TestDisplayName = "MyTest",
				TestCollectionUniqueID = collectionID,
				TestMethodUniqueID = methodID,
				TestUniqueID = testID
			};
			var testCleanupFailure = new _TestCleanupFailure
			{
				AssemblyUniqueID = assemblyID,
				ExceptionParentIndices = exceptionParentIndices,
				ExceptionTypes = exceptionTypes,
				Messages = messages,
				StackTraces = stackTraces,
				TestCaseUniqueID = testCaseID,
				TestCollectionUniqueID = collectionID,
				TestClassUniqueID = classID,
				TestMethodUniqueID = methodID,
				TestUniqueID = testID
			};
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(testStarting);
			handler.OnMessage(testCleanupFailure);

			AssertFailureMessages(handler.Messages, "Test Cleanup Failure (MyTest)");
		}

		[Fact]
		public void TestCollectionCleanupFailure()
		{
			var collectionStarting = new _TestCollectionStarting
			{
				AssemblyUniqueID = assemblyID,
				TestCollectionDisplayName = "FooBar",
				TestCollectionUniqueID = collectionID
			};
			var collectionCleanupFailure = new _TestCollectionCleanupFailure
			{
				AssemblyUniqueID = assemblyID,
				ExceptionParentIndices = exceptionParentIndices,
				ExceptionTypes = exceptionTypes,
				Messages = messages,
				StackTraces = stackTraces,
				TestCollectionUniqueID = collectionID
			};
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(collectionStarting);
			handler.OnMessage(collectionCleanupFailure);

			AssertFailureMessages(handler.Messages, "Test Collection Cleanup Failure (FooBar)");
		}

		[Fact]
		public void TestMethodCleanupFailure()
		{
			var methodStarting = new _TestMethodStarting
			{
				AssemblyUniqueID = assemblyID,
				TestClassUniqueID = classID,
				TestCollectionUniqueID = collectionID,
				TestMethod = "MyMethod",
				TestMethodUniqueID = methodID,
			};
			var methodCleanupFailure = new _TestMethodCleanupFailure
			{
				AssemblyUniqueID = assemblyID,
				ExceptionParentIndices = exceptionParentIndices,
				ExceptionTypes = exceptionTypes,
				Messages = messages,
				StackTraces = stackTraces,
				TestCollectionUniqueID = collectionID,
				TestClassUniqueID = classID,
				TestMethodUniqueID = methodID
			};
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(methodStarting);
			handler.OnMessage(methodCleanupFailure);

			AssertFailureMessages(handler.Messages, "Test Method Cleanup Failure (MyMethod)");
		}

		static void AssertFailureMessages(
			IEnumerable<string> messages,
			string messageType)
		{
#if NETCOREAPP  // Stack frame parsing appears to be broken outside of en-US culture on .NET Framework
			Assert.Collection(
				messages,
				msg => Assert.Equal("[Err @ SomeFolder\\SomeClass.cs:18] =>     [" + messageType + "] ExceptionType", msg),
				msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>       ExceptionType : This is my message \t", msg),
				msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>       Message Line 2", msg),
				msg => Assert.Equal("[--- @ SomeFolder\\SomeClass.cs:18] =>       Stack Trace:", msg),
				msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         Line 1", msg),
				msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         SomeFolder\\SomeClass.cs(18,0): at SomeClass.SomeMethod()", msg),
				msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         Line 3", msg)
			);
#endif
		}
	}

	public class OnMessage_TestAssemblyDiscoveryFinished
	{
		[Theory]
		[InlineData(false, 0, "[Imp] =>   Discovered:  test-assembly")]
		[InlineData(true, 42, "[Imp] =>   Discovered:  test-assembly (42 test cases to be run)")]
		[InlineData(true, 1, "[Imp] =>   Discovered:  test-assembly (1 test case to be run)")]
		[InlineData(true, 0, "[Imp] =>   Discovered:  test-assembly (0 test cases to be run)")]
		public static void LogsMessage(
			bool diagnosticMessages,
			int toRun,
			string expectedResult)
		{
			var message = TestData.TestAssemblyDiscoveryFinished(diagnosticMessages: diagnosticMessages, testCasesToRun: toRun);
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(message);

			var msg = Assert.Single(handler.Messages);
			Assert.Equal(expectedResult, msg);
		}
	}

	public class OnMessage_TestAssemblyDiscoveryStarting
	{
		[Theory]
		// If diagnostics messages are off, then it doesn't matter what app domain options we pass
		[InlineData(false, AppDomainOption.NotAvailable, false, "[Imp] =>   Discovering: test-assembly")]
		[InlineData(false, AppDomainOption.Disabled, false, "[Imp] =>   Discovering: test-assembly")]
		[InlineData(false, AppDomainOption.Enabled, false, "[Imp] =>   Discovering: test-assembly")]
		// If diagnostic messages are on, the message depends on what the app domain options say
		[InlineData(true, AppDomainOption.NotAvailable, false, "[Imp] =>   Discovering: test-assembly (method display = ClassAndMethod, method display options = None)")]
		[InlineData(true, AppDomainOption.Disabled, false, "[Imp] =>   Discovering: test-assembly (app domain = off, method display = ClassAndMethod, method display options = None)")]
		[InlineData(true, AppDomainOption.Enabled, false, "[Imp] =>   Discovering: test-assembly (app domain = on [no shadow copy], method display = ClassAndMethod, method display options = None)")]
		[InlineData(true, AppDomainOption.Enabled, true, "[Imp] =>   Discovering: test-assembly (app domain = on [shadow copy], method display = ClassAndMethod, method display options = None)")]
		public static void LogsMessage(
			bool diagnosticMessages,
			AppDomainOption appDomain,
			bool shadowCopy,
			string expectedResult)
		{
			var message = TestData.TestAssemblyDiscoveryStarting(diagnosticMessages: diagnosticMessages, appDomain: appDomain, shadowCopy: shadowCopy);
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(message);

			var msg = Assert.Single(handler.Messages);
			Assert.Equal(expectedResult, msg);
		}
	}

	public class OnMessage_TestAssemblyExecutionFinished
	{
		[Fact]
		public static void LogsMessage()
		{
			var message = TestData.TestAssemblyExecutionFinished();
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(message);

			var msg = Assert.Single(handler.Messages);
			Assert.Equal("[Imp] =>   Finished:    test-assembly", msg);
		}
	}

	public class OnMessage_TestAssemblyExecutionStarting
	{
		[Theory]
		[InlineData(false, null, null, null, null, null, "[Imp] =>   Starting:    test-assembly")]
		[InlineData(true, false, null, null, null, null, "[Imp] =>   Starting:    test-assembly (parallel test collections = off, stop on fail = off, explicit = only)")]
		[InlineData(true, null, -1, null, null, null, "[Imp] =>   Starting:    test-assembly (parallel test collections = on [unlimited threads], stop on fail = off, explicit = only)")]
		[InlineData(true, null, 1, null, null, null, "[Imp] =>   Starting:    test-assembly (parallel test collections = on [1 thread], stop on fail = off, explicit = only)")]
		[InlineData(true, null, null, true, null, null, "[Imp] =>   Starting:    test-assembly (parallel test collections = on [42 threads], stop on fail = on, explicit = only)")]
		[InlineData(true, null, null, null, null, null, "[Imp] =>   Starting:    test-assembly (parallel test collections = on [42 threads], stop on fail = off, explicit = only)")]
		[InlineData(true, null, null, null, 2112, null, "[Imp] =>   Starting:    test-assembly (parallel test collections = on [42 threads], stop on fail = off, explicit = only, seed = 2112)")]
		[InlineData(true, null, null, null, null, "", "[Imp] =>   Starting:    test-assembly (parallel test collections = on [42 threads], stop on fail = off, explicit = only, culture = invariant)")]
		[InlineData(true, null, null, null, null, "en-US", "[Imp] =>   Starting:    test-assembly (parallel test collections = on [42 threads], stop on fail = off, explicit = only, culture = en-US)")]
		public static void LogsMessage(
			bool diagnosticMessages,
			bool? parallelizeTestCollections,
			int? maxThreads,
			bool? stopOnFail,
			int? seed,
			string? culture,
			string expectedResult)
		{
			var message = TestData.TestAssemblyExecutionStarting(
				diagnosticMessages: diagnosticMessages,
				parallelizeTestCollections: parallelizeTestCollections,
				maxParallelThreads: maxThreads ?? 42,
				stopOnFail: stopOnFail,
				explicitOption: ExplicitOption.Only,
				seed: seed,
				culture: culture
			);
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(message);

			var msg = Assert.Single(handler.Messages);
			Assert.Equal(expectedResult, msg);
		}
	}

	public class OnMessage_ITestExecutionSummary
	{
		[CulturedFact]
		public void SingleAssembly()
		{
			var clockTime = TimeSpan.FromSeconds(12.3456);
			var summary = new ExecutionSummary { Total = 2112, Errors = 6, Failed = 42, Skipped = 8, NotRun = 3, Time = 1.2345M };
			var assemblyStartingMessage = TestData.TestAssemblyStarting(assemblyUniqueID: "asm-id", assemblyName: "assembly");
			var summaryMessage = TestData.TestExecutionSummaries(clockTime, "asm-id", summary);
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(assemblyStartingMessage);
			handler.OnMessage(summaryMessage);

			Assert.Collection(handler.Messages,
				msg => Assert.Equal($"[Imp] => === TEST EXECUTION SUMMARY ===", msg),
				msg => Assert.Equal($"[Imp] =>    assembly  Total: 2112, Errors: 6, Failed: 42, Skipped: 8, Not Run: 3, Time: {1.235m}s", msg)
			);
		}

		[CulturedFact]
		public void MultipleAssemblies()
		{
			var clockTime = TimeSpan.FromSeconds(12.3456);
			var @short = new ExecutionSummary { Total = 2112, Errors = 6, Failed = 42, Skipped = 8, NotRun = 3, Time = 1.2345M };
			var nothing = new ExecutionSummary { Total = 0 };
			var longerName = new ExecutionSummary { Total = 10240, Errors = 7, Failed = 96, Skipped = 4, NotRun = 7, Time = 3.4567M };
			var assemblyShortStarting = TestData.TestAssemblyStarting(assemblyUniqueID: "asm-short", assemblyName: "short");
			var assemblyNothingStarting = TestData.TestAssemblyStarting(assemblyUniqueID: "asm-nothing", assemblyName: "nothing");
			var assemblyLongerStarting = TestData.TestAssemblyStarting(assemblyUniqueID: "asm-longer", assemblyName: "longerName");
			var summaryMessage = TestData.TestExecutionSummaries(clockTime, ("asm-short", @short), ("asm-nothing", nothing), ("asm-longer", longerName));
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(assemblyShortStarting);
			handler.OnMessage(assemblyNothingStarting);
			handler.OnMessage(assemblyLongerStarting);
			handler.OnMessage(summaryMessage);

			Assert.Collection(handler.Messages,
				msg => Assert.Equal($"[Imp] => === TEST EXECUTION SUMMARY ===", msg),
				msg => Assert.Equal($"[Imp] =>    longerName  Total: 10240, Errors:  7, Failed:  96, Skipped:  4, Not Run:  7, Time: {3.457m}s", msg),
				msg => Assert.Equal($"[Imp] =>    nothing     Total:     0", msg),
				msg => Assert.Equal($"[Imp] =>    short       Total:  2112, Errors:  6, Failed:  42, Skipped:  8, Not Run:  3, Time: {1.235m}s", msg),
				msg => Assert.Equal($"[Imp] =>                       -----          --          ---           --           --        ------", msg),
				msg => Assert.Equal($"[Imp] =>          GRAND TOTAL: 12352          13          138           12           10        {4.691m}s ({12.346m}s)", msg)
			);
		}
	}

#if NETCOREAPP  // Stack frame parsing appears to be broken outside of en-US culture on .NET Framework
	public class OnMessage_TestFailed : DefaultRunnerReporterMessageHandlerTests
	{
		readonly _TestFailed failedMessage = TestData.TestFailed(
			exceptionParentIndices: FailureMessages.exceptionParentIndices,
			exceptionTypes: FailureMessages.exceptionTypes,
			output: $"This is\t{Environment.NewLine}output",
			messages: FailureMessages.messages,
			stackTraces: FailureMessages.stackTraces,
			warnings: new[] { "warning1", "warning2 line 1" + Environment.NewLine + "warning2 line 2" }
		);
		readonly _TestStarting startingMessage = TestData.TestStarting(testDisplayName: "This is my display name \t\r\n");

		[Fact]
		public void LogsTestNameWithExceptionAndStackTraceAndOutput()
		{
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(startingMessage);
			handler.OnMessage(failedMessage);

			Assert.Collection(
				handler.Messages,
				msg => Assert.Equal("[Err @ SomeFolder\\SomeClass.cs:18] =>     This is my display name \\t\\r\\n [FAIL]", msg),
				msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>       ExceptionType : This is my message \t", msg),
				msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>       Message Line 2", msg),
				msg => Assert.Equal("[--- @ SomeFolder\\SomeClass.cs:18] =>       Stack Trace:", msg),
				msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         Line 1", msg),
				msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         SomeFolder\\SomeClass.cs(18,0): at SomeClass.SomeMethod()", msg),
				msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         Line 3", msg),
				msg => Assert.Equal("[--- @ SomeFolder\\SomeClass.cs:18] =>       Output:", msg),
				msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         This is\t", msg),
				msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         output", msg),
				msg => Assert.Equal("[--- @ SomeFolder\\SomeClass.cs:18] =>       Warnings:", msg),
				msg => Assert.Equal("[Wrn @ SomeFolder\\SomeClass.cs:18] =>         • warning1", msg),
				msg => Assert.Equal("[Wrn @ SomeFolder\\SomeClass.cs:18] =>         • warning2 line 1", msg),
				msg => Assert.Equal("[Wrn @ SomeFolder\\SomeClass.cs:18] =>           warning2 line 2", msg)
			);
		}
	}
#endif

	public class OnMessage_TestPassed
	{
		readonly _TestPassed passedMessage = TestData.TestPassed(output: $"This is\t{Environment.NewLine}output");
		readonly _TestPassed passedMessageWithWarnings = TestData.TestPassed(warnings: new[] { "warning1", "warning2 line 1" + Environment.NewLine + "warning2 line 2" });
		readonly _TestStarting startingMessage = TestData.TestStarting(testDisplayName: "This is my display name \t\r\n");

		[Fact]
		public void LogsWarnings()
		{
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(startingMessage);
			handler.OnMessage(passedMessageWithWarnings);

			Assert.Collection(
				handler.Messages,
				msg => Assert.Equal("[Imp] =>     This is my display name \\t\\r\\n [PASS]", msg),
				msg => Assert.Equal("[---] =>       Warnings:", msg),
				msg => Assert.Equal("[Wrn] =>         • warning1", msg),
				msg => Assert.Equal("[Wrn] =>         • warning2 line 1", msg),
				msg => Assert.Equal("[Wrn] =>           warning2 line 2", msg)
			);
		}

		[Fact]
		public void DoesNotLogOutputByDefault()
		{
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(startingMessage);
			handler.OnMessage(passedMessage);

			Assert.Empty(handler.Messages);
		}

		[Fact]
		public void LogsOutputWhenDiagnosticsAreEnabled()
		{
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();
			handler.OnMessage(TestData.TestAssemblyExecutionStarting(diagnosticMessages: true));
			handler.OnMessage(TestData.TestAssemblyStarting());
			handler.Messages.Clear();  // Reset any output from previous messages

			handler.OnMessage(startingMessage);
			handler.OnMessage(passedMessage);

			Assert.Collection(
				handler.Messages,
				msg => Assert.Equal("[Imp] =>     This is my display name \\t\\r\\n [PASS]", msg),
				msg => Assert.Equal("[---] =>       Output:", msg),
				msg => Assert.Equal("[Imp] =>         This is\t", msg),
				msg => Assert.Equal("[Imp] =>         output", msg)
			);
		}
	}

	public class OnMessage_TestSkipped
	{
		[Theory]
		[InlineData("\r\n")]
		[InlineData("\r")]
		[InlineData("\n")]
		public static void LogsTestNameAsWarning(string skipNewline)
		{
			var startingMessage = TestData.TestStarting(testDisplayName: "This is my display name \t\r\n");
			var skipMessage = TestData.TestSkipped(
				reason: $"This is my skip reason \t{skipNewline}across multiple lines",
				warnings: new[] { "warning1", "warning2 line 1" + Environment.NewLine + "warning2 line 2" }
			);
			var handler = TestableDefaultRunnerReporterMessageHandler.Create();

			handler.OnMessage(startingMessage);
			handler.OnMessage(skipMessage);

			Assert.Collection(handler.Messages,
				msg => Assert.Equal("[Wrn] =>     This is my display name \\t\\r\\n [SKIP]", msg),
				msg => Assert.Equal($"[Imp] =>       This is my skip reason \t{Environment.NewLine}      across multiple lines", msg),
				msg => Assert.Equal("[---] =>       Warnings:", msg),
				msg => Assert.Equal("[Wrn] =>         • warning1", msg),
				msg => Assert.Equal("[Wrn] =>         • warning2 line 1", msg),
				msg => Assert.Equal("[Wrn] =>           warning2 line 2", msg)
			);
		}
	}

	// Helpers

	class TestableDefaultRunnerReporterMessageHandler : DefaultRunnerReporterMessageHandler
	{
		public List<string> Messages;

		TestableDefaultRunnerReporterMessageHandler(SpyRunnerLogger logger)
			: base(logger)
		{
			Messages = logger.Messages;
		}

		public static TestableDefaultRunnerReporterMessageHandler Create()
		{
			return new TestableDefaultRunnerReporterMessageHandler(new SpyRunnerLogger());
		}
	}
}
