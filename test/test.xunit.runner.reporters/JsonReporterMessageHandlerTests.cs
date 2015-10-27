using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
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
                yield return new[] { Mocks.TestSkipped("This is my display name \t\r\n", "This is my skip reason \t\r\n") };
                yield return new[] { Mocks.TestStarting("This is my display name \t\r\n") };
                yield return new[] { Mocks.TestPassed("This is my display name \t\r\n", "This is\t\r\noutput") };
                yield return new[] { Mocks.TestFailed("This is my display name \t\r\n", 1.2345M, "ExceptionType", "This is my message \t\r\n", "Line 1\r\nLine 2\r\nLine 3", "This is\t\r\noutput") };
                yield return new[] { Mocks.TestCollectionStarting() };
                yield return new[] { Mocks.TestCollectionFinished() };

                // IErrorMessage
                yield return new object[] { MakeFailureInformationSubstitute<IErrorMessage>()};

                // ITestAssemblyCleanupFailure
                var assemblyCleanupFailure = MakeFailureInformationSubstitute<ITestAssemblyCleanupFailure>();
                var testAssembly = Mocks.TestAssembly(@"C:\Foo\bar.dll");
                assemblyCleanupFailure.TestAssembly.Returns(testAssembly);
                yield return new object[] { assemblyCleanupFailure };

                // ITestCollectionCleanupFailure
                var collectionCleanupFailure = MakeFailureInformationSubstitute<ITestCollectionCleanupFailure>();
                var testCollection = Mocks.TestCollection(displayName: "FooBar");
                collectionCleanupFailure.TestCollection.Returns(testCollection);
                yield return new object[] { collectionCleanupFailure };

                // ITestClassCleanupFailure
                var classCleanupFailure = MakeFailureInformationSubstitute<ITestClassCleanupFailure>();
                var testClass = Mocks.TestClass("MyType");
                classCleanupFailure.TestClass.Returns(testClass);
                yield return new object[] { classCleanupFailure };

                // ITestMethodCleanupFailure
                var methodCleanupFailure = MakeFailureInformationSubstitute<ITestMethodCleanupFailure>();
                var testMethod = Mocks.TestMethod(methodName: "MyMethod");
                methodCleanupFailure.TestMethod.Returns(testMethod);
                yield return new object[] { methodCleanupFailure };

                // ITestCaseCleanupFailure
                var testCaseCleanupFailure = MakeFailureInformationSubstitute<ITestCaseCleanupFailure>();
                var testCase = Mocks.TestCase(typeof(object), "ToString", displayName: "MyTestCase");
                testCaseCleanupFailure.TestCase.Returns(testCase);
                yield return new object[] { testCaseCleanupFailure };

                // ITestCleanupFailure
                var testCleanupFailure = MakeFailureInformationSubstitute<ITestCleanupFailure>();
                var test = Mocks.Test(testCase, "MyTest");
                testCleanupFailure.Test.Returns(test);
                yield return new object[] { testCleanupFailure };

            }
        }

        [Theory]
        [MemberData("Messages")]
        public static void LogsMessage(IMessageSinkMessage message)
        {
            //Arrange
            var logger = Substitute.For<IRunnerLogger>();
            var jsonReporterMessageHandler = new JsonReporterMessageHandler(logger);
            string output = null;
            logger.LogImportantMessage(Arg.Do<string>(str => output = str));

            //Act
            jsonReporterMessageHandler.OnMessage(message);

            //Assert
            var json = new System.Web.Script.Serialization.JavaScriptSerializer().DeserializeObject(output);
        }
    }
}
