using System.Collections.Generic;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.Common;
using Xunit.v3;

public class TeamCityReporterMessageHandlerTests
{
	public class FailureMessages
	{
		readonly string assemblyID = "assembly-id";
		//readonly string classID = "test-class-id";
		readonly string collectionID = "test-collection-id";
		readonly int[] exceptionParentIndices = new[] { -1 };
		readonly string[] exceptionTypes = new[] { "\x2018ExceptionType\x2019" };
		readonly string[] messages = new[] { "This is my message \x2020\t\r\n" };
		//readonly string methodID = "test-method-id";
		readonly string[] stackTraces = new[] { "Line 1 \x0d60\r\nLine 2 \x1f64\r\nLine 3 \x999f" };
		//readonly string testCaseID = "test-case-id";
		//readonly string testID = "test-id";

		static TMessageType MakeFailureInformationSubstitute<TMessageType>()
			where TMessageType : class, IFailureInformation
		{
			var result = Substitute.For<TMessageType, InterfaceProxy<TMessageType>>();
			result.ExceptionTypes.Returns(new[] { "\x2018ExceptionType\x2019" });
			result.Messages.Returns(new[] { "This is my message \x2020\t\r\n" });
			result.StackTraces.Returns(new[] { "Line 1 \x0d60\r\nLine 2 \x1f64\r\nLine 3 \x999f" });
			return result;
		}

		public static IEnumerable<object[]> Messages
		{
			get
			{
				// IErrorMessage
				yield return new object[] { MakeFailureInformationSubstitute<IErrorMessage>(), "FATAL ERROR" };

				// ITestAssemblyCleanupFailure
				var assemblyCleanupFailure = MakeFailureInformationSubstitute<ITestAssemblyCleanupFailure>();
				var testAssembly = Mocks.TestAssembly(@"C:\Foo\bar.dll");
				assemblyCleanupFailure.TestAssembly.Returns(testAssembly);
				yield return new object[] { assemblyCleanupFailure, @"Test Assembly Cleanup Failure (C:\Foo\bar.dll)" };

				// ITestClassCleanupFailure
				var classCleanupFailure = MakeFailureInformationSubstitute<ITestClassCleanupFailure>();
				var testClass = Mocks.TestClass("MyType");
				classCleanupFailure.TestClass.Returns(testClass);
				yield return new object[] { classCleanupFailure, "Test Class Cleanup Failure (MyType)" };

				// ITestMethodCleanupFailure
				var methodCleanupFailure = MakeFailureInformationSubstitute<ITestMethodCleanupFailure>();
				var testMethod = Mocks.TestMethod(methodName: "MyMethod");
				methodCleanupFailure.TestMethod.Returns(testMethod);
				yield return new object[] { methodCleanupFailure, "Test Method Cleanup Failure (MyMethod)" };

				// ITestCaseCleanupFailure
				var testCaseCleanupFailure = MakeFailureInformationSubstitute<ITestCaseCleanupFailure>();
				var testCase = Mocks.TestCase(typeof(object), "ToString", displayName: "MyTestCase");
				testCaseCleanupFailure.TestCase.Returns(testCase);
				yield return new object[] { testCaseCleanupFailure, "Test Case Cleanup Failure (MyTestCase)" };

				// ITestCleanupFailure
				var testCleanupFailure = MakeFailureInformationSubstitute<ITestCleanupFailure>();
				var test = Mocks.Test(testCase, "MyTest");
				testCleanupFailure.Test.Returns(test);
				yield return new object[] { testCleanupFailure, "Test Cleanup Failure (MyTest)" };
			}
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
			var handler = TestableTeamCityReporterMessageHandler.Create();

			handler.OnMessage(collectionStarting);
			handler.OnMessage(collectionCleanupFailure);

			AssertFailureMessage(handler.Messages, "Test Collection Cleanup Failure (FooBar)");
		}

		[Theory]
		[MemberData(nameof(Messages), DisableDiscoveryEnumeration = true)]
		public static void LogsMessage(
			IMessageSinkMessage message,
			string messageType)
		{
			var handler = TestableTeamCityReporterMessageHandler.Create();

			handler.OnMessage(message);

			AssertFailureMessage(handler.Messages, messageType);
		}

		static void AssertFailureMessage(IEnumerable<string> messages, string messageType)
		{
			Assert.Contains(
				$"[Imp] => ##teamcity[message text='|[{messageType}|] |0x2018ExceptionType|0x2019: |0x2018ExceptionType|0x2019 : This is my message |0x2020\t|r|n' errorDetails='Line 1 |0x0d60|r|nLine 2 |0x1f64|r|nLine 3 |0x999f' status='ERROR']",
				messages
			);
		}
	}

	public class OnMessage_TestCollectionStarting_TestCollectionFinished
	{
		[Fact]
		public static void LogsMessage()
		{
			var startingMessage = Mocks.TestCollectionStarting(testCollectionUniqueID: "test-collection-id", testCollectionDisplayName: "my-test-collection");
			var finishedMessage = Mocks.TestCollectionFinished(testCollectionUniqueID: "test-collection-id");
			var handler = TestableTeamCityReporterMessageHandler.Create();

			handler.OnMessage(startingMessage);
			handler.OnMessage(finishedMessage);

			Assert.Collection(
				handler.Messages,
				msg => Assert.Equal("[Imp] => ##teamcity[testSuiteStarted name='my-test-collection (test-collection-id)' flowId='test-collection-id']", msg),
				msg => Assert.Equal("[Imp] => ##teamcity[testSuiteFinished name='my-test-collection (test-collection-id)' flowId='test-collection-id']", msg)
			);
		}
	}

	public class OnMessage_ITestFailed
	{
		[Fact]
		public static void LogsTestNameWithExceptionAndStackTraceAndOutput()
		{
			var message = Mocks.TestFailed("This is my display name \t\r\n", 1.2345M, "ExceptionType", "This is my message \t\r\n", "Line 1\r\nLine 2\r\nLine 3", "This is\t\r\noutput");
			var handler = TestableTeamCityReporterMessageHandler.Create();

			handler.OnMessage(message);

			Assert.Collection(
				handler.Messages,
				msg => Assert.Equal("[Imp] => ##teamcity[testFailed name='FORMATTED:This is my display name \t|r|n' details='ExceptionType : This is my message \t|r|n|r|nLine 1|r|nLine 2|r|nLine 3' flowId='myFlowId']", msg),
				msg => Assert.Equal("[Imp] => ##teamcity[testStdOut name='FORMATTED:This is my display name \t|r|n' out='This is\t|r|noutput' flowId='myFlowId']", msg),
				msg => Assert.Equal("[Imp] => ##teamcity[testFinished name='FORMATTED:This is my display name \t|r|n' duration='1234' flowId='myFlowId']", msg)
			);
		}
	}

	public class OnMessage_ITestPassed
	{
		[Fact]
		public static void LogsTestNameAndOutput()
		{
			var message = Mocks.TestPassed("This is my display name \t\r\n", "This is\t\r\noutput");
			var handler = TestableTeamCityReporterMessageHandler.Create();

			handler.OnMessage(message);

			Assert.Collection(
				handler.Messages,
				msg => Assert.Equal("[Imp] => ##teamcity[testStdOut name='FORMATTED:This is my display name \t|r|n' out='This is\t|r|noutput' flowId='myFlowId']", msg),
				msg => Assert.Equal("[Imp] => ##teamcity[testFinished name='FORMATTED:This is my display name \t|r|n' duration='1234' flowId='myFlowId']", msg)
			);
		}
	}

	public class OnMessage_ITestSkipped
	{
		[Fact]
		public static void LogsTestNameAsWarning()
		{
			var message = Mocks.TestSkipped("This is my display name \t\r\n", "This is my skip reason \t\r\n");
			var handler = TestableTeamCityReporterMessageHandler.Create();

			handler.OnMessage(message);

			Assert.Collection(
				handler.Messages,
				msg => Assert.Equal("[Imp] => ##teamcity[testIgnored name='FORMATTED:This is my display name \t|r|n' message='This is my skip reason \t|r|n' flowId='myFlowId']", msg),
				msg => Assert.Equal("[Imp] => ##teamcity[testFinished name='FORMATTED:This is my display name \t|r|n' duration='0' flowId='myFlowId']", msg)
			);
		}
	}

	public class OnMessage_ITestStarting
	{
		[Fact]
		public static void LogsTestName()
		{
			var message = Mocks.TestStarting("This is my display name \t\r\n");
			var handler = TestableTeamCityReporterMessageHandler.Create();

			handler.OnMessage(message);

			var msg = Assert.Single(handler.Messages);
			Assert.Equal(msg, "[Imp] => ##teamcity[testStarted name='FORMATTED:This is my display name \t|r|n' flowId='myFlowId']");
		}
	}

	// Helpers

	class TestableTeamCityReporterMessageHandler : TeamCityReporterMessageHandler
	{
		public IReadOnlyList<string> Messages;

		TestableTeamCityReporterMessageHandler(
			SpyRunnerLogger logger,
			TeamCityDisplayNameFormatter formatter)
				: base(logger, _ => "myFlowId", formatter)
		{
			Messages = logger.Messages;
		}

		public static TestableTeamCityReporterMessageHandler Create()
		{
			return new TestableTeamCityReporterMessageHandler(new SpyRunnerLogger(), new PassThroughFormatter());
		}

		class PassThroughFormatter : TeamCityDisplayNameFormatter
		{
			public override string DisplayName(ITestCollection testCollection) => $"FORMATTED:{testCollection.DisplayName}";

			public override string DisplayName(ITest test) => $"FORMATTED:{test.DisplayName}";
		}
	}
}
