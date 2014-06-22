﻿using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.MSBuild;

public class StandardOutputVisitorTests
{
    public class OnMessage_ErrorMessage
    {
        [Fact]
        public void LogsMessage()
        {
            var errorMessage = Substitute.For<IErrorMessage>();
            errorMessage.ExceptionTypes.Returns(new[] { "ExceptionType" });
            errorMessage.Messages.Returns(new[] { "This is my message \t\r\n" });
            errorMessage.StackTraces.Returns(new[] { "Line 1\r\nLine 2\r\nLine 3" });

            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null, false, null);

            visitor.OnMessage(errorMessage);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("ERROR: ExceptionType : This is my message \\t\\r\\n", msg),
                msg => Assert.Equal("ERROR: Line 1\r\nLine 2\r\nLine 3", msg));
        }
    }

    public class OnMessage_TestAssemblyFinished
    {
        [Fact]
        public void LogsMessageWithStatitics()
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
            var visitor = new StandardOutputVisitor(logger, null, false, null);

            visitor.OnMessage(assemblyStarting);
            visitor.OnMessage(assemblyFinished);

            Assert.Single(logger.Messages, "MESSAGE[High]:   Started: File.dll");
            Assert.Single(logger.Messages, "MESSAGE[High]:   Finished: File.dll");
        }
    }

    public class OnMessage_TestFailed
    {
        [Fact]
        public void LogsTestNameWithExceptionAndStackTrace()
        {
            var testFailed = Substitute.For<ITestFailed>();
            testFailed.TestDisplayName.Returns("This is my display name \t\r\n");
            testFailed.Messages.Returns(new[] { "This is my message \t\r\n" });
            testFailed.StackTraces.Returns(new[] { "Line 1\r\nLine 2\r\nLine 3" });
            testFailed.ExceptionTypes.Returns(new[] { "ExceptionType" });
            testFailed.ExceptionParentIndices.Returns(new[] { -1 });

            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null, false, null);

            visitor.OnMessage(testFailed);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("ERROR: This is my display name \\t\\r\\n: ExceptionType : This is my message \\t\\r\\n", msg),
                msg => Assert.Equal("ERROR: Line 1\r\nLine 2\r\nLine 3", msg));
        }

        [Fact]
        public void NullStackTraceDoesNotLogStackTrace()
        {
            var testFailed = Substitute.For<ITestFailed>();
            testFailed.TestDisplayName.Returns("1");
            testFailed.Messages.Returns(new[] { "2" });
            testFailed.StackTraces.Returns(new[] { (string)null });
            testFailed.ExceptionTypes.Returns(new[] { "ExceptionType" });
            testFailed.ExceptionParentIndices.Returns(new[] { -1 });

            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null, false, null);

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
            var visitor = new StandardOutputVisitor(logger, null, false, null);

            visitor.OnMessage(testPassed);

            Assert.Single(logger.Messages, "MESSAGE[Normal]:     This is my display name \\t\\r\\n");
        }

        [Fact]
        public void AddsPassToLogWhenInVerboseMode()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null, true, null);

            visitor.OnMessage(testPassed);

            Assert.Single(logger.Messages, "MESSAGE[Normal]:     PASS:  This is my display name \\t\\r\\n");
        }
    }

    public class OnMessage_TestSkipped
    {
        [Fact]
        public void LogsTestNameAsWarning()
        {
            var testSkipped = Substitute.For<ITestSkipped>();
            testSkipped.TestDisplayName.Returns("This is my display name \t\r\n");
            testSkipped.Reason.Returns("This is my skip reason \t\r\n");

            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null, false, null);

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
            var visitor = new StandardOutputVisitor(logger, null, false, null);

            visitor.OnMessage(testStarting);

            Assert.Empty(logger.Messages);
        }

        [Fact]
        public void OutputStartMessageWhenInVerboseMode()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null, true, null);

            visitor.OnMessage(testStarting);

            Assert.Single(logger.Messages, "MESSAGE[Normal]:     START: This is my display name \\t\\r\\n");
        }
    }
}