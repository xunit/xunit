using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.MSBuild;

public class StandardOutputVisitorTests
{
    public class OnMessage_ErrorMessage
    {
        IErrorMessage errorMessage;

        public OnMessage_ErrorMessage()
        {
            errorMessage = Substitute.For<IErrorMessage>();
            errorMessage.ExceptionType.Returns("ExceptionType");
            errorMessage.Message.Returns("This is my message \t\r\n");
            errorMessage.StackTrace.Returns("Line 1\r\nLine 2\r\nLine 3");
        }

        [Fact]
        public void LogsMessage()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, null);

            var result = visitor.OnMessage(errorMessage);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("ERROR: ExceptionType: This is my message \\t\\r\\n", msg),
                msg => Assert.Equal("ERROR: Line 1\r\nLine 2\r\nLine 3", msg));
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, () => true);

            var result = visitor.OnMessage(errorMessage);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, () => false);

            var result = visitor.OnMessage(errorMessage);

            Assert.True(result);
        }
    }

    public class OnMessage_TestAssemblyFinished
    {
        ITestAssemblyFinished assemblyFinished;

        public OnMessage_TestAssemblyFinished()
        {
            assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            assemblyFinished.TestsRun.Returns(2112);
            assemblyFinished.TestsFailed.Returns(42);
            assemblyFinished.TestsSkipped.Returns(6);
            assemblyFinished.ExecutionTime.Returns(123.4567M);
        }

        [Fact]
        public void LogsMessageWithStatitics()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, null);

            visitor.OnMessage(assemblyFinished);

            Assert.Single(logger.Messages, "MESSAGE[High]:   Tests: 2112, Failures: 42, Skipped: 6, Time: 123.457 seconds");
        }

        [Fact]
        public void AddsStatisticsToRunningTotal()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, null) { Total = 10, Failed = 10, Skipped = 10, Time = 10M };

            visitor.OnMessage(assemblyFinished);

            Assert.Equal(2122, visitor.Total);
            Assert.Equal(52, visitor.Failed);
            Assert.Equal(16, visitor.Skipped);
            Assert.Equal(133.4567M, visitor.Time);
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, () => true);

            var result = visitor.OnMessage(assemblyFinished);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, () => false);

            var result = visitor.OnMessage(assemblyFinished);

            Assert.True(result);
        }
    }

    public class OnMessage_TestFailed
    {
        ITestFailed testFailed;

        public OnMessage_TestFailed()
        {
            testFailed = Substitute.For<ITestFailed>();
            testFailed.TestDisplayName.Returns("This is my display name \t\r\n");
            testFailed.Message.Returns("This is my message \t\r\n");
            testFailed.StackTrace.Returns("Line 1\r\nLine 2\r\nLine 3");
        }

        [Fact]
        public void LogsTestNameWithExceptionAndStackTrace()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, null);

            visitor.OnMessage(testFailed);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("ERROR: This is my display name \\t\\r\\n: This is my message \\t\\r\\n", msg),
                msg => Assert.Equal("ERROR: Line 1\r\nLine 2\r\nLine 3", msg));
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, () => true);

            var result = visitor.OnMessage(testFailed);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, () => false);

            var result = visitor.OnMessage(testFailed);

            Assert.True(result);
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
            var visitor = new StandardOutputVisitor(logger, false, null);

            visitor.OnMessage(testPassed);

            Assert.Single(logger.Messages, "MESSAGE[Normal]:     This is my display name \\t\\r\\n");
        }

        [Fact]
        public void AddsPassToLogWhenInVerboseMode()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, true, null);

            visitor.OnMessage(testPassed);

            Assert.Single(logger.Messages, "MESSAGE[Normal]:     PASS:  This is my display name \\t\\r\\n");
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, () => true);

            var result = visitor.OnMessage(testPassed);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, () => false);

            var result = visitor.OnMessage(testPassed);

            Assert.True(result);
        }
    }

    public class OnMessage_TestSkipped
    {
        ITestSkipped testSkipped;

        public OnMessage_TestSkipped()
        {
            testSkipped = Substitute.For<ITestSkipped>();
            testSkipped.TestDisplayName.Returns("This is my display name \t\r\n");
            testSkipped.Reason.Returns("This is my skip reason \t\r\n");
        }

        [Fact]
        public void LogsTestNameAsWarning()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, null);

            visitor.OnMessage(testSkipped);

            Assert.Single(logger.Messages, "WARNING: This is my display name \\t\\r\\n: This is my skip reason \\t\\r\\n");
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, () => true);

            var result = visitor.OnMessage(testSkipped);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, () => false);

            var result = visitor.OnMessage(testSkipped);

            Assert.True(result);
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
            var visitor = new StandardOutputVisitor(logger, false, null);

            visitor.OnMessage(testStarting);

            Assert.Empty(logger.Messages);
        }

        [Fact]
        public void OutputStartMessageWhenInVerboseMode()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, true, null);

            visitor.OnMessage(testStarting);

            Assert.Single(logger.Messages, "MESSAGE[Normal]:     START: This is my display name \\t\\r\\n");
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, () => true);

            var result = visitor.OnMessage(testStarting);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, false, () => false);

            var result = visitor.OnMessage(testStarting);

            Assert.True(result);
        }
    }
}