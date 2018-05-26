using System;
using System.Collections.Generic;
using System.Globalization;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public class DefaultRunnerReporterWithTypesMessageHandlerTests
{
    static void SetupFailureInformation(IFailureInformation failureInfo)
    {
        failureInfo.ExceptionTypes.Returns(new[] { "ExceptionType" });
        failureInfo.Messages.Returns(new[] { "This is my message \t\r\nMessage Line 2" });
        failureInfo.StackTraces.Returns(new[] { "Line 1\r\nat SomeClass.SomeMethod() in SomeFolder\\SomeClass.cs:line 18\r\nLine 3" });
    }

    public class FailureMessages
    {
        static TMessageType MakeFailureInformationSubstitute<TMessageType>()
            where TMessageType : class, IFailureInformation
        {
            var message = Substitute.For<TMessageType, InterfaceProxy<TMessageType>>();
            SetupFailureInformation(message);
            return message;
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

                // ITestCollectionCleanupFailure
                var collectionCleanupFailure = MakeFailureInformationSubstitute<ITestCollectionCleanupFailure>();
                var testCollection = Mocks.TestCollection(displayName: "FooBar");
                collectionCleanupFailure.TestCollection.Returns(testCollection);
                yield return new object[] { collectionCleanupFailure, "Test Collection Cleanup Failure (FooBar)" };

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

        [Theory]
        [MemberData("Messages")]
        public void LogsMessage(IMessageSinkMessage message, string messageType)
        {
            var handler = TestableDefaultRunnerReporterWithTypesMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            Assert.Collection(handler.Messages,
                msg => Assert.Equal("[Err @ SomeFolder\\SomeClass.cs:18] =>     [" + messageType + "] ExceptionType", msg),
                msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>       ExceptionType : This is my message \t", msg),
                msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>       Message Line 2", msg),
                msg => Assert.Equal("[--- @ SomeFolder\\SomeClass.cs:18] =>       Stack Trace:", msg),
                msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         Line 1", msg),
                msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         SomeFolder\\SomeClass.cs(18,0): at SomeClass.SomeMethod()", msg),
                msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         Line 3", msg)
            );
        }
    }

    public class OnMessage_ITestAssemblyDiscoveryFinished
    {
        [Theory]
        [InlineData(false, 0, 0, "[Imp] =>   Discovered:  testAssembly")]
        [InlineData(true, 42, 2112, "[Imp] =>   Discovered:  testAssembly (found 42 of 2112 test cases)")]
        [InlineData(true, 42, 42, "[Imp] =>   Discovered:  testAssembly (found 42 test cases)")]
        [InlineData(true, 1, 1, "[Imp] =>   Discovered:  testAssembly (found 1 test case)")]
        [InlineData(true, 0, 1, "[Imp] =>   Discovered:  testAssembly (found 0 of 1 test cases)")]
        public static void LogsMessage(bool diagnosticMessages, int toRun, int discovered, string expectedResult)
        {
            var message = Mocks.TestAssemblyDiscoveryFinished(diagnosticMessages, toRun, discovered);
            var handler = TestableDefaultRunnerReporterWithTypesMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            var msg = Assert.Single(handler.Messages);
            Assert.Equal(expectedResult, msg);
        }
    }

    public class OnMessage_ITestAssemblyDiscoveryStarting
    {
        [Theory]
        [InlineData(false, "[Imp] =>   Discovering: testAssembly")]
#if NETFRAMEWORK
        [InlineData(true, "[Imp] =>   Discovering: testAssembly (app domain = on [no shadow copy], method display = ClassAndMethod, method display options = None)")]
#else
        [InlineData(true, "[Imp] =>   Discovering: testAssembly (method display = ClassAndMethod, method display options = None)")]
#endif
        public static void LogsMessage(bool diagnosticMessages, string expectedResult)
        {
            var message = Mocks.TestAssemblyDiscoveryStarting(diagnosticMessages: diagnosticMessages, appDomain: true);
            var handler = TestableDefaultRunnerReporterWithTypesMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            var msg = Assert.Single(handler.Messages);
            Assert.Equal(expectedResult, msg);
        }
    }

    public class OnMessage_ITestAssemblyExecutionFinished
    {
        [Fact]
        public static void LogsMessage()
        {
            var message = Mocks.TestAssemblyExecutionFinished();
            var handler = TestableDefaultRunnerReporterWithTypesMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            var msg = Assert.Single(handler.Messages);
            Assert.Equal("[Imp] =>   Finished:    testAssembly", msg);
        }
    }

    public class OnMessage_ITestAssemblyExecutionStarting
    {
        [Theory]
        [InlineData(false, "[Imp] =>   Starting:    testAssembly")]
        [InlineData(true, "[Imp] =>   Starting:    testAssembly (parallel test collections = on, max threads = 42)")]
        public static void LogsMessage(bool diagnosticMessages, string expectedResult)
        {
            var message = Mocks.TestAssemblyExecutionStarting(diagnosticMessages: diagnosticMessages);
            var handler = TestableDefaultRunnerReporterWithTypesMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            var msg = Assert.Single(handler.Messages);
            Assert.Equal(expectedResult, msg);
        }
    }

    public class OnMessage_ITestExecutionSummary
    {
        [CulturedFact("en-US")]
        public void SingleAssembly()
        {
            var clockTime = TimeSpan.FromSeconds(12.3456);
            var assembly = new ExecutionSummary { Total = 2112, Errors = 6, Failed = 42, Skipped = 8, Time = 1.2345M };
            var message = new TestExecutionSummary(clockTime, new List<KeyValuePair<string, ExecutionSummary>> { new KeyValuePair<string, ExecutionSummary>("assembly", assembly) });
            var handler = TestableDefaultRunnerReporterWithTypesMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            Assert.Collection(handler.Messages,
                msg => Assert.Equal("[Imp] => === TEST EXECUTION SUMMARY ===", msg),
                msg => Assert.Equal("[Imp] =>    assembly  Total: 2112, Errors: 6, Failed: 42, Skipped: 8, Time: 1.235s", msg)
            );
        }

        [CulturedFact("en-US")]
        public void MultipleAssemblies()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

            var clockTime = TimeSpan.FromSeconds(12.3456);
            var @short = new ExecutionSummary { Total = 2112, Errors = 6, Failed = 42, Skipped = 8, Time = 1.2345M };
            var nothing = new ExecutionSummary { Total = 0 };
            var longerName = new ExecutionSummary { Total = 10240, Errors = 7, Failed = 96, Skipped = 4, Time = 3.4567M };
            var message = new TestExecutionSummary(clockTime, new List<KeyValuePair<string, ExecutionSummary>> {
                new KeyValuePair<string, ExecutionSummary>("short", @short),
                new KeyValuePair<string, ExecutionSummary>("nothing", nothing),
                new KeyValuePair<string, ExecutionSummary>("longerName", longerName),
            });
            var handler = TestableDefaultRunnerReporterWithTypesMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            Assert.Collection(handler.Messages,
                msg => Assert.Equal("[Imp] => === TEST EXECUTION SUMMARY ===", msg),
                msg => Assert.Equal("[Imp] =>    short       Total:  2112, Errors:  6, Failed:  42, Skipped:  8, Time: 1.235s", msg),
                msg => Assert.Equal("[Imp] =>    nothing     Total:     0", msg),
                msg => Assert.Equal("[Imp] =>    longerName  Total: 10240, Errors:  7, Failed:  96, Skipped:  4, Time: 3.457s", msg),
                msg => Assert.Equal("[Imp] =>                       -----          --          ---           --        ------", msg),
                msg => Assert.Equal("[Imp] =>          GRAND TOTAL: 12352          13          138           12        4.691s (12.346s)", msg)
            );
        }
    }

    public class OnMessage_ITestFailed : DefaultRunnerReporterMessageHandlerTests
    {
        [Fact]
        public void LogsTestNameWithExceptionAndStackTraceAndOutput()
        {
            var message = Mocks.TestFailed("This is my display name \t\r\n", 1.2345M, output: "This is\t" + Environment.NewLine + "output");
            SetupFailureInformation(message);
            var handler = TestableDefaultRunnerReporterWithTypesMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            Assert.Collection(handler.Messages,
                msg => Assert.Equal("[Err @ SomeFolder\\SomeClass.cs:18] =>     This is my display name \\t\\r\\n [FAIL]", msg),
                msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>       ExceptionType : This is my message \t", msg),
                msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>       Message Line 2", msg),
                msg => Assert.Equal("[--- @ SomeFolder\\SomeClass.cs:18] =>       Stack Trace:", msg),
                msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         Line 1", msg),
                msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         SomeFolder\\SomeClass.cs(18,0): at SomeClass.SomeMethod()", msg),
                msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         Line 3", msg),
                msg => Assert.Equal("[--- @ SomeFolder\\SomeClass.cs:18] =>       Output:", msg),
                msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         This is\t", msg),
                msg => Assert.Equal("[Imp @ SomeFolder\\SomeClass.cs:18] =>         output", msg)
            );
        }
    }

    public class OnMessage_ITestPassed
    {
        [Fact]
        public void DoesNotLogOutputByDefault()
        {
            var message = Mocks.TestPassed("This is my display name \t\r\n", output: "This is\t" + Environment.NewLine + "output");
            var handler = TestableDefaultRunnerReporterWithTypesMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            Assert.Empty(handler.Messages);
        }

        [Fact]
        public void LogsOutputWhenDiagnosticsAreEnabled()
        {
            var message = Mocks.TestPassed("This is my display name \t\r\n", output: "This is\t" + Environment.NewLine + "output");
            var handler = TestableDefaultRunnerReporterWithTypesMessageHandler.Create();
            handler.OnMessageWithTypes(Mocks.TestAssemblyExecutionStarting(diagnosticMessages: true, assemblyFilename: message.TestAssembly.Assembly.AssemblyPath), null);
            handler.Messages.Clear();  // Ignore any output from the "assembly execution starting" message

            handler.OnMessageWithTypes(message, null);

            Assert.Collection(handler.Messages,
                msg => Assert.Equal("[Imp] =>     This is my display name \\t\\r\\n [PASS]", msg),
                msg => Assert.Equal("[---] =>       Output:", msg),
                msg => Assert.Equal("[Imp] =>         This is\t", msg),
                msg => Assert.Equal("[Imp] =>         output", msg)
            );
        }
    }

    public class OnMessage_ITestSkipped
    {
        [Fact]
        public static void LogsTestNameAsWarning()
        {
            var message = Mocks.TestSkipped("This is my display name \t\r\n", "This is my skip reason \t\r\n");
            var handler = TestableDefaultRunnerReporterWithTypesMessageHandler.Create();

            handler.OnMessageWithTypes(message, null);

            Assert.Collection(handler.Messages,
                msg => Assert.Equal("[Wrn] =>     This is my display name \\t\\r\\n [SKIP]", msg),
                msg => Assert.Equal("[Imp] =>       This is my skip reason \\t\\r\\n", msg)
            );
        }
    }

    // Helpers

    class TestableDefaultRunnerReporterWithTypesMessageHandler : DefaultRunnerReporterWithTypesMessageHandler
    {
        public List<string> Messages;

        TestableDefaultRunnerReporterWithTypesMessageHandler(SpyRunnerLogger logger)
            : base(logger)
        {
            Messages = logger.Messages;
        }

        public static TestableDefaultRunnerReporterWithTypesMessageHandler Create()
        {
            return new TestableDefaultRunnerReporterWithTypesMessageHandler(new SpyRunnerLogger());
        }
    }
}
