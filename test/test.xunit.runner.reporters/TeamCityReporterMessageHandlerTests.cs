using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.Reporters;

public class TeamCityReporterMessageHandlerTests
{
    public class FailureMessages
    {
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
                yield return new object[] { MakeFailureInformationSubstitute<IErrorMessage>(), "FATAL ERROR", null };

                // ITestAssemblyCleanupFailure
                var assemblyCleanupFailure = MakeFailureInformationSubstitute<ITestAssemblyCleanupFailure>();
                var testAssembly = Mocks.TestAssembly(@"C:\Foo\bar.dll");
                assemblyCleanupFailure.TestAssembly.Returns(testAssembly);
                yield return new object[] { assemblyCleanupFailure, @"Test Assembly Cleanup Failure (C:|0x005CFoo|0x005Cbar.dll)", "C:|0x005CFoo|0x005Cbar.dll" };

                // ITestCollectionCleanupFailure
                var collectionCleanupFailure = MakeFailureInformationSubstitute<ITestCollectionCleanupFailure>();
                var testCollection = Mocks.TestCollection(displayName: "FooBar\t\r\n", uniqueID: Guid.Empty);
                collectionCleanupFailure.TestCollection.Returns(testCollection);
                yield return new object[] { collectionCleanupFailure, "Test Collection Cleanup Failure (FooBar\t|r|n (00000000000000000000000000000000))", "00000000000000000000000000000000" };

                // ITestClassCleanupFailure
                var classCleanupFailure = MakeFailureInformationSubstitute<ITestClassCleanupFailure>();
                var testClass = Mocks.TestClass("MyType\t\r\n");
                classCleanupFailure.TestClass.Returns(testClass);
                yield return new object[] { classCleanupFailure, "Test Class Cleanup Failure (MyType\t|r|n)", "00000000000000000000000000000000" };

                // ITestMethodCleanupFailure
                var methodCleanupFailure = MakeFailureInformationSubstitute<ITestMethodCleanupFailure>();
                var testMethod = Mocks.TestMethod(typeName: "MyClass\t\r\n", methodName: "MyMethod\t\r\n");
                methodCleanupFailure.TestMethod.Returns(testMethod);
                yield return new object[] { methodCleanupFailure, "Test Method Cleanup Failure (MyClass\t|r|n.MyMethod\t|r|n)", "00000000000000000000000000000000" };

                // ITestCaseCleanupFailure
                var testCaseCleanupFailure = MakeFailureInformationSubstitute<ITestCaseCleanupFailure>();
                var testCase = Mocks.TestCase(typeof(object), "ToString", displayName: "MyTestCase\t\r\n");
                testCaseCleanupFailure.TestCase.Returns(testCase);
                yield return new object[] { testCaseCleanupFailure, "Test Case Cleanup Failure (MyTestCase\t|r|n)", "00000000000000000000000000000000" };

                // ITestCleanupFailure
                var testCleanupFailure = MakeFailureInformationSubstitute<ITestCleanupFailure>();
                var test = Mocks.Test(testCase, "MyTest\t\r\n");
                testCleanupFailure.Test.Returns(test);
                yield return new object[] { testCleanupFailure, "Test Cleanup Failure (MyTest\t|r|n)", "00000000000000000000000000000000" };
            }
        }

        [Theory]
        [MemberData("Messages")]
        public static void LogsMessage(IMessageSinkMessage message, string messageType, string flowId)
        {
            var handler = TestableTeamCityReporterMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            var msg = Assert.Single(handler.Messages.Where(msg => msg.Contains("##teamcity")));
            Assert.Equal($"[Raw] => ##teamcity[message timestamp='2023-05-03T21:12:00.000+0000'{(flowId == null ? "" : $" flowId='{flowId}'")} status='ERROR' text='|[{messageType}|] |0x2018ExceptionType|0x2019: |0x2018ExceptionType|0x2019 : This is my message |0x2020\t|r|n' errorDetails='Line 1 |0x0d60|r|nLine 2 |0x1f64|r|nLine 3 |0x999f']", msg);
        }
    }

    public class OnMessage_ITestAssemblyStarting_ITestAssemblyFinished
    {
        [Fact]
        public static void StartsAndEndsFlowAndSuite()
        {
            var startingMessage = Mocks.TestAssemblyStarting(assemblyFileName: @"/path/to\test-assembly.exe");
            var finishedMessage = Mocks.TestAssemblyFinished(assemblyFileName: @"/path/to\test-assembly.exe");
            var handler = TestableTeamCityReporterMessageHandler.Create();

            handler.OnMessage(startingMessage);
            handler.OnMessage(finishedMessage);

            Assert.Collection(
                handler.Messages.Where(msg => msg.Contains("##teamcity")),
                msg => Assert.Equal("[Raw] => ##teamcity[flowStarted timestamp='2023-05-03T21:12:00.000+0000' flowId='/path/to|0x005Ctest-assembly.exe']", msg),
                msg => Assert.Equal("[Raw] => ##teamcity[testSuiteStarted timestamp='2023-05-03T21:12:00.000+0000' flowId='/path/to|0x005Ctest-assembly.exe' name='/path/to|0x005Ctest-assembly.exe']", msg),
                msg => Assert.Equal("[Raw] => ##teamcity[testSuiteFinished timestamp='2023-05-03T21:12:00.000+0000' flowId='/path/to|0x005Ctest-assembly.exe' name='/path/to|0x005Ctest-assembly.exe']", msg),
                msg => Assert.Equal("[Raw] => ##teamcity[flowFinished timestamp='2023-05-03T21:12:00.000+0000' flowId='/path/to|0x005Ctest-assembly.exe']", msg)
            );
        }

        [Fact]
        public static void FallsBackToAssemblyNameWhenPathIsNull()
        {
            var startingMessage = Mocks.TestAssemblyStarting(assemblyFileName: null, assemblyName: "test[assembly]");
            var finishedMessage = Mocks.TestAssemblyFinished(assemblyFileName: null, assemblyName: "test[assembly]");
            var handler = TestableTeamCityReporterMessageHandler.Create();

            handler.OnMessage(startingMessage);
            handler.OnMessage(finishedMessage);

            Assert.Collection(
                handler.Messages.Where(msg => msg.Contains("##teamcity")),
                msg => Assert.Equal("[Raw] => ##teamcity[flowStarted timestamp='2023-05-03T21:12:00.000+0000' flowId='test|[assembly|]']", msg),
                msg => Assert.Equal("[Raw] => ##teamcity[testSuiteStarted timestamp='2023-05-03T21:12:00.000+0000' flowId='test|[assembly|]' name='test|[assembly|]']", msg),
                msg => Assert.Equal("[Raw] => ##teamcity[testSuiteFinished timestamp='2023-05-03T21:12:00.000+0000' flowId='test|[assembly|]' name='test|[assembly|]']", msg),
                msg => Assert.Equal("[Raw] => ##teamcity[flowFinished timestamp='2023-05-03T21:12:00.000+0000' flowId='test|[assembly|]']", msg)
            );
        }

        [Fact]
        public static void UsesRootFlowIDFromTeamCityEnvironment()
        {
            var startingMessage = Mocks.TestAssemblyStarting(assemblyFileName: @"/path/to\test-assembly.exe");
            var finishedMessage = Mocks.TestAssemblyFinished(assemblyFileName: @"/path/to\test-assembly.exe");
            var handler = TestableTeamCityReporterMessageHandler.Create("root-flow-id\t\r\n");

            handler.OnMessage(startingMessage);
            handler.OnMessage(finishedMessage);

            var msg = handler.Messages.Where(msg => msg.Contains("##teamcity")).First();
            Assert.Equal("[Raw] => ##teamcity[flowStarted timestamp='2023-05-03T21:12:00.000+0000' flowId='/path/to|0x005Ctest-assembly.exe' parent='root-flow-id\t|r|n']", msg);
        }
    }

    public class OnMessage_ITestCollectionStarting_ITestCollectionFinished
    {
        [Fact]
        public static void StartsAndEndsFlowAndSuite()
        {
            var startingMessage = Mocks.TestCollectionStarting("my-test-collection\t\r\n", assemblyFileName: "test[assembly].dll");
            var finishedMessage = Mocks.TestCollectionFinished("my-test-collection\t\r\n", assemblyFileName: "test[assembly].dll");
            var handler = TestableTeamCityReporterMessageHandler.Create();

            handler.OnMessageWithTypes(startingMessage, null);
            handler.OnMessageWithTypes(finishedMessage, null);

            Assert.Collection(
                handler.Messages.Where(msg => msg.Contains("##teamcity")),
                msg => Assert.Equal("[Raw] => ##teamcity[flowStarted timestamp='2023-05-03T21:12:00.000+0000' flowId='00000000000000000000000000000000' parent='test|[assembly|].dll']", msg),
                msg => Assert.Equal("[Raw] => ##teamcity[testSuiteStarted timestamp='2023-05-03T21:12:00.000+0000' flowId='00000000000000000000000000000000' name='my-test-collection\t|r|n (00000000000000000000000000000000)']", msg),
                msg => Assert.Equal("[Raw] => ##teamcity[testSuiteFinished timestamp='2023-05-03T21:12:00.000+0000' flowId='00000000000000000000000000000000' name='my-test-collection\t|r|n (00000000000000000000000000000000)']", msg),
                msg => Assert.Equal("[Raw] => ##teamcity[flowFinished timestamp='2023-05-03T21:12:00.000+0000' flowId='00000000000000000000000000000000']", msg)
            );
        }
    }

    public class OnMessage_ITestFailed
    {
        [Fact]
        public static void LogsTestFailed()
        {
            var message = Mocks.TestFailed("This is my display name \t\r\n", 1.2345M, "ExceptionType", "This is my message \t\r\n", "Line 1\r\nLine 2\r\nLine 3", "This is\t\r\noutput");
            var handler = TestableTeamCityReporterMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            var msg = Assert.Single(handler.Messages.Where(msg => msg.Contains("##teamcity")));
            Assert.Equal("[Raw] => ##teamcity[testFailed timestamp='2023-05-03T21:12:00.000+0000' flowId='00000000000000000000000000000000' name='This is my display name \t|r|n' details='ExceptionType : This is my message \t|r|n|r|nLine 1|r|nLine 2|r|nLine 3']", msg);
        }
    }

    public class OnMessage_ITestFinished
    {
        [Fact]
        public static void WithoutOutput()
        {
            var message = Mocks.TestFinished("This is my display name \t\r\n", executionTime: 1.234m);
            var handler = TestableTeamCityReporterMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            var msg = Assert.Single(handler.Messages.Where(msg => msg.Contains("##teamcity")));
            Assert.Equal("[Raw] => ##teamcity[testFinished timestamp='2023-05-03T21:12:00.000+0000' flowId='00000000000000000000000000000000' name='This is my display name \t|r|n' duration='1234']", msg);
        }

        [Fact]
        public static void WithOutput()
        {
            var message = Mocks.TestFinished("This is my display name \t\r\n", "This is\t\r\noutput", 1.234m);
            var handler = TestableTeamCityReporterMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            Assert.Collection(handler.Messages.Where(msg => msg.Contains("##teamcity")),
                msg => Assert.Equal("[Raw] => ##teamcity[testStdOut timestamp='2023-05-03T21:12:00.000+0000' flowId='00000000000000000000000000000000' name='This is my display name \t|r|n' out='This is\t|r|noutput' tc:tags='tc:parseServiceMessagesInside']]", msg),
                msg => Assert.Equal("[Raw] => ##teamcity[testFinished timestamp='2023-05-03T21:12:00.000+0000' flowId='00000000000000000000000000000000' name='This is my display name \t|r|n' duration='1234']", msg)
            );
        }
    }

    public class OnMessage_ITestSkipped
    {
        [Fact]
        public static void LogsTestIgnored()
        {
            var message = Mocks.TestSkipped("This is my display name \t\r\n", "This is my skip reason \t\r\n");
            var handler = TestableTeamCityReporterMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            var msg = Assert.Single(handler.Messages.Where(msg => msg.Contains("##teamcity")));
            Assert.Equal("[Raw] => ##teamcity[testIgnored timestamp='2023-05-03T21:12:00.000+0000' flowId='00000000000000000000000000000000' name='This is my display name \t|r|n' message='This is my skip reason \t|r|n']", msg);
        }
    }

    public class OnMessage_ITestStarting
    {
        [Fact]
        public static void LogsTestStarted()
        {
            var message = Mocks.TestStarting("This is my display name \t\r\n");
            var handler = TestableTeamCityReporterMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            var msg = Assert.Single(handler.Messages.Where(msg => msg.Contains("##teamcity")));
            Assert.Equal("[Raw] => ##teamcity[testStarted timestamp='2023-05-03T21:12:00.000+0000' flowId='00000000000000000000000000000000' name='This is my display name \t|r|n']", msg);
        }
    }

    // Helpers

    class TestableTeamCityReporterMessageHandler : TeamCityReporterMessageHandler
    {
        DateTimeOffset now = new DateTimeOffset(2023, 5, 3, 21, 12, 0, TimeSpan.Zero);

        public IReadOnlyList<string> Messages;

        TestableTeamCityReporterMessageHandler(SpyRunnerLogger logger, string rootFlowId)
            : base(logger, rootFlowId)
        {
            Messages = logger.Messages;
        }

        protected override DateTimeOffset UtcNow => now;

        public static TestableTeamCityReporterMessageHandler Create(string rootFlowId = null)
        {
            return new TestableTeamCityReporterMessageHandler(new SpyRunnerLogger(), rootFlowId);
        }
    }
}
