using Moq;
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
            var mockMessage = new Mock<IErrorMessage>();
            mockMessage.Setup(em => em.ExceptionType).Returns("ExceptionType");
            mockMessage.Setup(em => em.Message).Returns("This is my message \t\r\n");
            mockMessage.Setup(em => em.StackTrace).Returns("Line 1\r\nLine 2\r\nLine 3");
            errorMessage = mockMessage.Object;
        }

        [Fact]
        public void LogsMessage()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null);

            var result = visitor.OnMessage(errorMessage);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("ERROR: ExceptionType: This is my message \\t\\r\\n", msg),
                msg => Assert.Equal("ERROR: Line 1\r\nLine 2\r\nLine 3", msg));
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, () => true);

            var result = visitor.OnMessage(errorMessage);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, () => false);

            var result = visitor.OnMessage(errorMessage);

            Assert.True(result);
        }
    }

    public class OnMessage_TestAssemblyFinished
    {
        ITestAssemblyFinished assemblyFinished;

        public OnMessage_TestAssemblyFinished()
        {
            var mockMessage = new Mock<ITestAssemblyFinished>();
            mockMessage.Setup(af => af.TestsRun).Returns(2112);
            mockMessage.Setup(af => af.TestsFailed).Returns(42);
            mockMessage.Setup(af => af.TestsSkipped).Returns(6);
            mockMessage.Setup(af => af.ExecutionTime).Returns(123.4567M);
            assemblyFinished = mockMessage.Object;
        }

        [Fact]
        public void LogsMessageWithStatitics()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null);

            visitor.OnMessage(assemblyFinished);

            Assert.Single(logger.Messages, "MESSAGE[High]:   Tests: 2112, Failures: 42, Skipped: 6, Time: 123.457 seconds");
        }

        [Fact]
        public void AddsStatisticsToRunningTotal()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null) { Total = 10, Failed = 10, Skipped = 10, Time = 10M };

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
            var visitor = new StandardOutputVisitor(logger, () => true);

            var result = visitor.OnMessage(assemblyFinished);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, () => false);

            var result = visitor.OnMessage(assemblyFinished);

            Assert.True(result);
        }
    }

    public class OnMessage_TestFailed
    {
        ITestFailed testFailed;

        public OnMessage_TestFailed()
        {
            var mockMessage = new Mock<ITestFailed>();
            mockMessage.Setup(tf => tf.TestDisplayName).Returns("This is my display name \t\r\n");
            mockMessage.Setup(tf => tf.Message).Returns("This is my message \t\r\n");
            mockMessage.Setup(tf => tf.StackTrace).Returns("Line 1\r\nLine 2\r\nLine 3");
            testFailed = mockMessage.Object;
        }

        [Fact]
        public void LogsTestNameWithExceptionAndStackTrace()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null);

            visitor.OnMessage(testFailed);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("ERROR: This is my display name \\t\\r\\n: This is my message \\t\\r\\n", msg),
                msg => Assert.Equal("ERROR: Line 1\r\nLine 2\r\nLine 3", msg));
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, () => true);

            var result = visitor.OnMessage(testFailed);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, () => false);

            var result = visitor.OnMessage(testFailed);

            Assert.True(result);
        }
    }

    public class OnMessage_TestPassed
    {
        ITestPassed testPassed;

        public OnMessage_TestPassed()
        {
            var mockMessage = new Mock<ITestPassed>();
            mockMessage.Setup(tp => tp.TestDisplayName).Returns("This is my display name \t\r\n");
            testPassed = mockMessage.Object;
        }

        [Fact]
        public void LogsTestName()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null);

            visitor.OnMessage(testPassed);

            Assert.Single(logger.Messages, "MESSAGE[Normal]:     This is my display name \\t\\r\\n");
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, () => true);

            var result = visitor.OnMessage(testPassed);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, () => false);

            var result = visitor.OnMessage(testPassed);

            Assert.True(result);
        }
    }

    public class OnMessage_TestSkipped
    {
        ITestSkipped testSkipped;

        public OnMessage_TestSkipped()
        {
            var mockMessage = new Mock<ITestSkipped>();
            mockMessage.Setup(ts => ts.TestDisplayName).Returns("This is my display name \t\r\n");
            mockMessage.Setup(ts => ts.Reason).Returns("This is my skip reason \t\r\n");
            testSkipped = mockMessage.Object;
        }

        [Fact]
        public void LogsTestNameAsWarning()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null);

            visitor.OnMessage(testSkipped);

            Assert.Single(logger.Messages, "WARNING: This is my display name \\t\\r\\n: This is my skip reason \\t\\r\\n");
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, () => true);

            var result = visitor.OnMessage(testSkipped);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, () => false);

            var result = visitor.OnMessage(testSkipped);

            Assert.True(result);
        }
    }
}