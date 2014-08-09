using System;
using System.Collections.Generic;
using System.Xml.Linq;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.MSBuild;

public class StandardOutputVisitorTests
{
    public class OnMessage_ErrorInformationMessages
    {
        static TMessageType MakeFailureInformationSubstitute<TMessageType>()
            where TMessageType : class, IFailureInformation
        {
            var result = Substitute.For<TMessageType>();
            result.ExceptionTypes.Returns(new[] { "ExceptionType" });
            result.Messages.Returns(new[] { "This is my message \t\r\n" });
            result.StackTraces.Returns(new[] { "Line 1\r\nLine 2\r\nLine 3" });
            return result;
        }

        public static IEnumerable<object[]> Messages
        {
            get
            {
                yield return new object[] { MakeFailureInformationSubstitute<IErrorMessage>(), "FATAL" };

                var assemblyCleanupFailure = MakeFailureInformationSubstitute<ITestAssemblyCleanupFailure>();
                var testAssembly = Mocks.TestAssembly(@"C:\Foo\bar.dll");
                assemblyCleanupFailure.TestAssembly.Returns(testAssembly);
                yield return new object[] { assemblyCleanupFailure, @"Test Assembly Cleanup Failure (C:\Foo\bar.dll)" };

                var collectionCleanupFailure = MakeFailureInformationSubstitute<ITestCollectionCleanupFailure>();
                var testCollection = Mocks.TestCollection(displayName: "FooBar");
                collectionCleanupFailure.TestCollection.Returns(testCollection);
                yield return new object[] { collectionCleanupFailure, "Test Collection Cleanup Failure (FooBar)" };

                var classCleanupFailure = MakeFailureInformationSubstitute<ITestClassCleanupFailure>();
                var testClass = Mocks.TestClass("MyType");
                classCleanupFailure.TestClass.Returns(testClass);
                yield return new object[] { classCleanupFailure, "Test Class Cleanup Failure (MyType)" };

                var methodCleanupFailure = MakeFailureInformationSubstitute<ITestMethodCleanupFailure>();
                var testMethod = Mocks.TestMethod(methodName: "MyMethod");
                methodCleanupFailure.TestMethod.Returns(testMethod);
                yield return new object[] { methodCleanupFailure, "Test Method Cleanup Failure (MyMethod)" };

                var testCaseCleanupFailure = MakeFailureInformationSubstitute<ITestCaseCleanupFailure>();
                var testCase = Mocks.TestCase(typeof(Object), "ToString", displayName: "MyTestCase");
                testCaseCleanupFailure.TestCase.Returns(testCase);
                yield return new object[] { testCaseCleanupFailure, "Test Case Cleanup Failure (MyTestCase)" };

                var testCleanupFailure = MakeFailureInformationSubstitute<ITestCleanupFailure>();
                testCleanupFailure.TestDisplayName.Returns("MyTest");
                yield return new object[] { testCleanupFailure, "Test Cleanup Failure (MyTest)" };
            }
        }

        [Theory]
        [MemberData("Messages")]
        public static void LogsMessage(IMessageSinkMessage message, string messageType)
        {
            var logger = SpyLogger.Create();
            using (var visitor = new StandardOutputVisitor(logger, new XElement("assembly"), false, null))
            {
                visitor.OnMessage(message);

                Assert.Collection(logger.Messages,
                    msg => Assert.Equal(String.Format("ERROR: [{0}] ExceptionType : This is my message \\t\\r\\n", messageType), msg),
                    msg => Assert.Equal("ERROR: Line 1\r\nLine 2\r\nLine 3", msg));
            }
        }

        [Theory]
        [MemberData("Messages")]
        public static void IncludesSourceLineNumberFromTopOfStack(IMessageSinkMessage message, string messageType)
        {
            ((IFailureInformation)message).StackTraces.Returns(new[] { @"   at FixtureAcceptanceTests.Constructors.TestClassMustHaveSinglePublicConstructor() in d:\Dev\xunit\xunit\test\test.xunit.execution\Acceptance\FixtureAcceptanceTests.cs:line 16" });

            var logger = SpyLogger.Create(includeSourceInformation: true);
            using (var visitor = new StandardOutputVisitor(logger, new XElement("assembly"), false, null))
            {
                visitor.OnMessage(message);

                Assert.Collection(logger.Messages,
                    msg => Assert.Equal(String.Format(@"ERROR: [FILE d:\Dev\xunit\xunit\test\test.xunit.execution\Acceptance\FixtureAcceptanceTests.cs][LINE 16] [{0}] ExceptionType : This is my message \t\r\n", messageType), msg),
                    msg => Assert.Equal(@"ERROR: [FILE d:\Dev\xunit\xunit\test\test.xunit.execution\Acceptance\FixtureAcceptanceTests.cs][LINE 16]    at FixtureAcceptanceTests.Constructors.TestClassMustHaveSinglePublicConstructor() in d:\Dev\xunit\xunit\test\test.xunit.execution\Acceptance\FixtureAcceptanceTests.cs:line 16", msg));
            }
        }

        [Theory]
        [MemberData("Messages")]
        public static void IncludesSourceLineNumberOfFirstStackFrameWithSourceInformation(IMessageSinkMessage message, string messageType)
        {
            ((IFailureInformation)message).StackTraces.Returns(new[] { @"   at System.Linq.Enumerable.Single[TSource](IEnumerable`1 source)" + Environment.NewLine
                                                                     + @"   at FixtureAcceptanceTests.ClassFixture.TestClassWithExtraArgumentToConstructorResultsInFailedTest() in d:\Dev\xunit\xunit\test\test.xunit.execution\Acceptance\FixtureAcceptanceTests.cs:line 76" });

            var logger = SpyLogger.Create(includeSourceInformation: true);
            using (var visitor = new StandardOutputVisitor(logger, new XElement("assembly"), false, null))
            {
                visitor.OnMessage(message);

                Assert.Collection(logger.Messages,
                    msg => Assert.Equal(String.Format(@"ERROR: [FILE d:\Dev\xunit\xunit\test\test.xunit.execution\Acceptance\FixtureAcceptanceTests.cs][LINE 76] [{0}] ExceptionType : This is my message \t\r\n", messageType), msg),
                    msg => Assert.Equal(String.Format(@"ERROR: [FILE d:\Dev\xunit\xunit\test\test.xunit.execution\Acceptance\FixtureAcceptanceTests.cs][LINE 76]    at System.Linq.Enumerable.Single[TSource](IEnumerable`1 source){0}   at FixtureAcceptanceTests.ClassFixture.TestClassWithExtraArgumentToConstructorResultsInFailedTest() in d:\Dev\xunit\xunit\test\test.xunit.execution\Acceptance\FixtureAcceptanceTests.cs:line 76", Environment.NewLine), msg));
            }
        }
    }

    public class OnMessage_TestAssemblyFinished
    {
        [Fact]
        public static void LogsMessageWithStatitics()
        {
            var assembly = Mocks.TestAssembly(@"C:\Assembly\File.dll");
            var assemblyStarting = Substitute.For<ITestAssemblyStarting>();
            assemblyStarting.TestAssembly.Returns(assembly);
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            assemblyFinished.TestsRun.Returns(2112);
            assemblyFinished.TestsFailed.Returns(42);
            assemblyFinished.TestsSkipped.Returns(6);
            assemblyFinished.ExecutionTime.Returns(123.4567M);

            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, new XElement("assembly"), false, null);

            visitor.OnMessage(assemblyStarting);
            visitor.OnMessage(assemblyFinished);

            Assert.Single(logger.Messages, "MESSAGE[High]:   Started:  File.dll");
            Assert.Single(logger.Messages, "MESSAGE[High]:   Finished: File.dll");
        }
    }

    public class OnMessage_TestFailed
    {
        [Fact]
        public static void LogsTestNameWithExceptionAndStackTrace()
        {
            var testFailed = Substitute.For<ITestFailed>();
            testFailed.TestDisplayName.Returns("This is my display name \t\r\n");
            testFailed.Messages.Returns(new[] { "This is my message \t\r\n" });
            testFailed.StackTraces.Returns(new[] { "Line 1\r\nLine 2\r\nLine 3" });
            testFailed.ExceptionTypes.Returns(new[] { "ExceptionType" });
            testFailed.ExceptionParentIndices.Returns(new[] { -1 });

            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, new XElement("assembly"), false, null);

            visitor.OnMessage(testFailed);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("ERROR: This is my display name \\t\\r\\n: ExceptionType : This is my message \\t\\r\\n", msg),
                msg => Assert.Equal("ERROR: Line 1\r\nLine 2\r\nLine 3", msg));
        }

        [Fact]
        public static void NullStackTraceDoesNotLogStackTrace()
        {
            var testFailed = Substitute.For<ITestFailed>();
            testFailed.TestDisplayName.Returns("1");
            testFailed.Messages.Returns(new[] { "2" });
            testFailed.StackTraces.Returns(new[] { (string)null });
            testFailed.ExceptionTypes.Returns(new[] { "ExceptionType" });
            testFailed.ExceptionParentIndices.Returns(new[] { -1 });

            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, new XElement("assembly"), false, null);

            visitor.OnMessage(testFailed);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("ERROR: 1: ExceptionType : 2", msg));
        }
    }

    public class OnMessage_TestPassed
    {
        ITestPassed testPassed;

        public OnMessage_TestPassed()
        {
            testPassed = Substitute.For<ITestPassed>();
            testPassed.TestDisplayName.Returns("This is my display name \t\r\n");
        }

        [Fact]
        public void LogsTestName()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, new XElement("assembly"), false, null);

            visitor.OnMessage(testPassed);

            Assert.Single(logger.Messages, "MESSAGE[Normal]:     This is my display name \\t\\r\\n");
        }

        [Fact]
        public void AddsPassToLogWhenInVerboseMode()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, new XElement("assembly"), true, null);

            visitor.OnMessage(testPassed);

            Assert.Single(logger.Messages, "MESSAGE[Normal]:     PASS:  This is my display name \\t\\r\\n");
        }
    }

    public class OnMessage_TestSkipped
    {
        [Fact]
        public static void LogsTestNameAsWarning()
        {
            var testSkipped = Substitute.For<ITestSkipped>();
            testSkipped.TestDisplayName.Returns("This is my display name \t\r\n");
            testSkipped.Reason.Returns("This is my skip reason \t\r\n");

            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, new XElement("assembly"), false, null);

            visitor.OnMessage(testSkipped);

            Assert.Single(logger.Messages, "WARNING: This is my display name \\t\\r\\n: This is my skip reason \\t\\r\\n");
        }
    }

    public class OnMessage_TestStarting
    {
        ITestStarting testStarting;

        public OnMessage_TestStarting()
        {
            testStarting = Substitute.For<ITestStarting>();
            testStarting.TestDisplayName.Returns("This is my display name \t\r\n");
        }

        [Fact]
        public void NoOutputWhenNotInVerboseMode()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, new XElement("assembly"), false, null);

            visitor.OnMessage(testStarting);

            Assert.Empty(logger.Messages);
        }

        [Fact]
        public void OutputStartMessageWhenInVerboseMode()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, new XElement("assembly"), true, null);

            visitor.OnMessage(testStarting);

            Assert.Single(logger.Messages, "MESSAGE[Normal]:     START: This is my display name \\t\\r\\n");
        }
    }
}