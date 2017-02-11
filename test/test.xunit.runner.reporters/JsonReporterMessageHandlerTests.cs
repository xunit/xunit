using System.Collections.Generic;
using NSubstitute;
using Xunit.Abstractions;

namespace Xunit.Runner.Reporters
{
    public class JsonReporterMessageHandlerTests
    {
        static TMessageType MakeFailureInformationSubstitute<TMessageType>()
            where TMessageType : class, IFailureInformation
        {
            var result = Substitute.For<TMessageType, InterfaceProxy<TMessageType>>();
            result.ExceptionTypes.Returns(new[] { "ExceptionType" });
            result.Messages.Returns(new[] { "This is my message \t\r\n" });
            result.StackTraces.Returns(new[] { "Line 1\r\nLine 2\r\nLine 3" });
            return result;
        }

        public static IEnumerable<object[]> Messages
        {
            get
            {
                yield return new object[]
                {
                    Mocks.TestSkipped("This is my display name \t\r\n", "This is my skip reason \t\r\n"),
                    @"{""message"":""testSkipped"",""flowId"":""mappedFlow"",""executionTime"":0,""output"":"""",""testName"":""This is my display name \t\r\n"",""reason"":""This is my skip reason \t\r\n""}"
                };
                yield return new object[]
                {
                    Mocks.TestStarting("This is my display name \t\r\n"),
                    @"{""message"":""testStarting"",""flowId"":""mappedFlow"",""testName"":""This is my display name \t\r\n""}"
                };
                yield return new object[]
                {
                    Mocks.TestPassed("This is my display name \t\r\n", "This is\t\r\noutput"),
                    @"{""message"":""testPassed"",""flowId"":""mappedFlow"",""executionTime"":1.2345,""output"":""This is\t\r\noutput""}"
                };
                yield return new object[]
                {
                    Mocks.TestFailed("This is my display name \t\r\n", 1.2345M, "ExceptionType", "This is my message \t\r\n", "Line 1\r\nLine 2\r\nLine 3", "This is\t\r\noutput"),
                    @"{""message"":""testFailed"",""flowId"":""mappedFlow"",""executionTime"":1.2345,""output"":""This is\t\r\noutput"",""errorMessages"":""ExceptionType : This is my message \t\r\n"",""stackTraces"":""Line 1\r\nLine 2\r\nLine 3"",""testName"":""This is my display name \t\r\n""}"
                };
                yield return new object[]
                {
                    Mocks.TestCollectionStarting(),
                    @"{""message"":""testCollectionStarting"",""flowId"":""mappedFlow"",""assembly"":"""",""collectionName"":""Display Name"",""collectionId"":""00000000-0000-0000-0000-000000000000""}"
                };
                yield return new object[]
                {
                    Mocks.TestCollectionFinished(),
                    @"{""message"":""testCollectionFinished"",""flowId"":""mappedFlow"",""assembly"":"""",""collectionName"":""Display Name"",""collectionId"":""00000000-0000-0000-0000-000000000000"",""executionTime"":123.4567,""testsFailed"":42,""testsRun"":2112,""testsSkipped"":6}"
                };

                // IErrorMessage
                yield return new object[]
                {
                    MakeFailureInformationSubstitute<IErrorMessage>(),
                    @"{""message"":""fatalError"",""errorMessages"":""ExceptionType : This is my message \t\r\n"",""stackTraces"":""Line 1\r\nLine 2\r\nLine 3""}"
                };

                // ITestAssemblyCleanupFailure
                var assemblyCleanupFailure = MakeFailureInformationSubstitute<ITestAssemblyCleanupFailure>();
                var testAssembly = Mocks.TestAssembly(@"C:\Foo\bar.dll");
                assemblyCleanupFailure.TestAssembly.Returns(testAssembly);
                yield return new object[]
                {
                    assemblyCleanupFailure,
                    @"{""message"":""testAssemblyCleanupFailure"",""errorMessages"":""ExceptionType : This is my message \t\r\n"",""stackTraces"":""Line 1\r\nLine 2\r\nLine 3""}"
                };

                // ITestCollectionCleanupFailure
                var collectionCleanupFailure = MakeFailureInformationSubstitute<ITestCollectionCleanupFailure>();
                var testCollection = Mocks.TestCollection(displayName: "FooBar");
                collectionCleanupFailure.TestCollection.Returns(testCollection);
                yield return new object[]
                {
                    collectionCleanupFailure,
                    @"{""message"":""testCollectionCleanupFailure"",""errorMessages"":""ExceptionType : This is my message \t\r\n"",""stackTraces"":""Line 1\r\nLine 2\r\nLine 3"",""assembly"":"""",""collectionName"":""FooBar"",""collectionId"":""" + testCollection.UniqueID + @"""}"
                };

                // ITestClassCleanupFailure
                var classCleanupFailure = MakeFailureInformationSubstitute<ITestClassCleanupFailure>();
                var testClass = Mocks.TestClass("MyType");
                classCleanupFailure.TestClass.Returns(testClass);
                yield return new object[]
                {
                    classCleanupFailure,
                    @"{""message"":""testClassCleanupFailure"",""errorMessages"":""ExceptionType : This is my message \t\r\n"",""stackTraces"":""Line 1\r\nLine 2\r\nLine 3""}"
                };

                // ITestMethodCleanupFailure
                var methodCleanupFailure = MakeFailureInformationSubstitute<ITestMethodCleanupFailure>();
                var testMethod = Mocks.TestMethod(methodName: "MyMethod");
                methodCleanupFailure.TestMethod.Returns(testMethod);
                yield return new object[]
                {
                    methodCleanupFailure,
                    @"{""message"":""testMethodCleanupFailure"",""errorMessages"":""ExceptionType : This is my message \t\r\n"",""stackTraces"":""Line 1\r\nLine 2\r\nLine 3""}"
                };

                // ITestCaseCleanupFailure
                var testCaseCleanupFailure = MakeFailureInformationSubstitute<ITestCaseCleanupFailure>();
                var testCase = Mocks.TestCase(typeof(object), "ToString", displayName: "MyTestCase");
                testCaseCleanupFailure.TestCase.Returns(testCase);
                yield return new object[]
                {
                    testCaseCleanupFailure,
                    @"{""message"":""testAssemblyCleanupFailure"",""errorMessages"":""ExceptionType : This is my message \t\r\n"",""stackTraces"":""Line 1\r\nLine 2\r\nLine 3""}"
                };

                // ITestCleanupFailure
                var testCleanupFailure = MakeFailureInformationSubstitute<ITestCleanupFailure>();
                var test = Mocks.Test(testCase, "MyTest");
                testCleanupFailure.Test.Returns(test);
                yield return new object[]
                {
                    testCleanupFailure,
                    @"{""message"":""testCleanupFailure"",""errorMessages"":""ExceptionType : This is my message \t\r\n"",""stackTraces"":""Line 1\r\nLine 2\r\nLine 3""}"
                };
            }
        }

        [Theory]
        [MemberData("Messages")]
        public static void LogsMessage(IMessageSinkMessage message, string expectedJson)
        {
            var logger = Substitute.For<IRunnerLogger>();
            var jsonReporterMessageHandler = new JsonReporterMessageHandler(logger, _ => $"mappedFlow");
            string output = null;
            logger.LogImportantMessage(Arg.Do<string>(str => output = str));

            jsonReporterMessageHandler.OnMessageWithTypes(message, null);

            Assert.Equal(expectedJson, output);
        }
    }
}
