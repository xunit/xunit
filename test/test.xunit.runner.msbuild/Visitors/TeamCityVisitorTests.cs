﻿using System;
using System.Collections.Generic;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.MSBuild;

public class TeamCityVisitorTests
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
                var test = Mocks.Test(testCase, "MyTest");
                testCleanupFailure.Test.Returns(test);
                yield return new object[] { testCleanupFailure, "Test Cleanup Failure (MyTest)" };
            }
        }

        [Theory]
        [MemberData("Messages")]
        public static void LogsMessage(IMessageSinkMessage message, string messageType)
        {
            var logger = SpyLogger.Create();
            using (var visitor = new TeamCityVisitor(logger, null, null))
            {
                visitor.OnMessage(message);

                var msg = Assert.Single(logger.Messages);
                Assert.Equal(String.Format("MESSAGE[High]: ##teamcity[message text='|[{0}|] ExceptionType: ExceptionType : This is my message \t|r|n' errorDetails='Line 1|r|nLine 2|r|nLine 3' status='ERROR']", messageType), msg);
            }
        }
    }

    public class OnMessage_TestCollectionFinished
    {
        [Fact]
        public void LogsMessage()
        {
            var collectionFinished = Substitute.For<ITestCollectionFinished>();
            collectionFinished.TestsRun.Returns(2112);
            collectionFinished.TestsFailed.Returns(42);
            collectionFinished.TestsSkipped.Returns(6);
            collectionFinished.ExecutionTime.Returns(123.4567M);
            collectionFinished.TestCollection.DisplayName.Returns("Display Name");

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null, _ => "myFlowId");

            visitor.OnMessage(collectionFinished);

            Assert.Single(logger.Messages, @"MESSAGE[High]: ##teamcity[testSuiteFinished name='Display Name (1)' flowId='myFlowId']");
        }
    }

    public class OnMessage_TestCollectionStarting
    {
        [Fact]
        public void LogsMessage()
        {
            var collectionStarting = Substitute.For<ITestCollectionStarting>();
            collectionStarting.TestCollection.DisplayName.Returns("Display Name");

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null, _ => "myFlowId");

            visitor.OnMessage(collectionStarting);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal(@"MESSAGE[High]: ##teamcity[testSuiteStarted name='Display Name (1)' flowId='myFlowId']", msg));
        }
    }

    public class OnMessage_TestFailed
    {
        [Fact]
        public void LogsTestNameWithExceptionAndStackTrace()
        {
            var testFailed = Substitute.For<ITestFailed>();
            var test = Mocks.Test(null, "This is my display name \t\r\n");
            testFailed.Test.Returns(test);
            testFailed.ExecutionTime.Returns(1.2345M);
            testFailed.Messages.Returns(new[] { "This is my message \t\r\n" });
            testFailed.StackTraces.Returns(new[] { "Line 1\r\nLine 2\r\nLine 3" });
            testFailed.ExceptionTypes.Returns(new[] { "ExceptionType" });
            testFailed.ExceptionParentIndices.Returns(new[] { -1 });

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null, _ => "myFlowId");

            visitor.OnMessage(testFailed);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testFailed name='This is my display name \t|r|n' details='ExceptionType : This is my message \t|r|n|r|nLine 1|r|nLine 2|r|nLine 3' flowId='myFlowId']", msg),
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testFinished name='This is my display name \t|r|n' duration='1234' flowId='myFlowId']", msg)
            );
        }
    }

    public class OnMessage_TestPassed
    {
        [Fact]
        public void LogsTestName()
        {
            var testPassed = Substitute.For<ITestPassed>();
            var test = Mocks.Test(null, "This is my display name \t\r\n");
            testPassed.Test.Returns(test);
            testPassed.ExecutionTime.Returns(1.2345M);

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null, _ => "myFlowId");

            visitor.OnMessage(testPassed);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testFinished name='This is my display name \t|r|n' duration='1234' flowId='myFlowId']", msg)
            );
        }
    }

    public class OnMessage_TestSkipped
    {
        [Fact]
        public void LogsTestNameAsWarning()
        {
            var testSkipped = Substitute.For<ITestSkipped>();
            var test = Mocks.Test(null, "This is my display name \t\r\n");
            testSkipped.Test.Returns(test);
            testSkipped.Reason.Returns("This is my skip reason \t\r\n");

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null, _ => "myFlowId");

            visitor.OnMessage(testSkipped);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testIgnored name='This is my display name \t|r|n' message='This is my skip reason \t|r|n' flowId='myFlowId']", msg),
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testFinished name='This is my display name \t|r|n' duration='0' flowId='myFlowId']", msg)
            );
        }
    }

    public class OnMessage_TestStarting
    {
        [Fact]
        public void LogsTestName()
        {
            var testStarting = Substitute.For<ITestStarting>();
            var test = Mocks.Test(null, "This is my display name \t\r\n");
            testStarting.Test.Returns(test);

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null, _ => "myFlowId");

            visitor.OnMessage(testStarting);

            Assert.Single(logger.Messages, "MESSAGE[High]: ##teamcity[testStarted name='This is my display name \t|r|n' flowId='myFlowId']");
        }
    }
}