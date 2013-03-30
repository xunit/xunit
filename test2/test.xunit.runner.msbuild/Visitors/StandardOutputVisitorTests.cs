using Moq;
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
            var errorMessage = new Mock<IErrorMessage>();
            errorMessage.Setup(em => em.ExceptionType).Returns("ExceptionType");
            errorMessage.Setup(em => em.Message).Returns("This is my message \t\r\n");
            errorMessage.Setup(em => em.StackTrace).Returns("Line 1\r\nLine 2\r\nLine 3");
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null);

            visitor.OnMessage(errorMessage.Object);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("ERROR: ExceptionType: This is my message \\t\\r\\n", msg),
                msg => Assert.Equal("ERROR: Line 1\r\nLine 2\r\nLine 3", msg));
        }
    }

    public class OnMessage_TestAssemblyFinished
    {
        [Fact]
        public void LogsMessageWithStatitics()
        {
            var assemblyFinished = new Mock<ITestAssemblyFinished>();
            assemblyFinished.Setup(af => af.TestsRun).Returns(2112);
            assemblyFinished.Setup(af => af.TestsFailed).Returns(42);
            assemblyFinished.Setup(af => af.TestsSkipped).Returns(6);
            assemblyFinished.Setup(af => af.ExecutionTime).Returns(123.4567M);
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null);

            visitor.OnMessage(assemblyFinished.Object);

            Assert.Single(logger.Messages, "MESSAGE[High]:   Tests: 2112, Failures: 42, Skipped: 6, Time: 123.457 seconds");
        }

        [Fact]
        public void AddsStatisticsToRunningTotal()
        {
            var assemblyFinished = new Mock<ITestAssemblyFinished>();
            assemblyFinished.Setup(af => af.TestsRun).Returns(2112);
            assemblyFinished.Setup(af => af.TestsFailed).Returns(42);
            assemblyFinished.Setup(af => af.TestsSkipped).Returns(6);
            assemblyFinished.Setup(af => af.ExecutionTime).Returns(123.4567M);
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null) { Total = 10, Failed = 10, Skipped = 10, Time = 10M };

            visitor.OnMessage(assemblyFinished.Object);

            Assert.Equal(2122, visitor.Total);
            Assert.Equal(52, visitor.Failed);
            Assert.Equal(16, visitor.Skipped);
            Assert.Equal(133.4567M, visitor.Time);
        }
    }

    public class OnMessage_TestFailed
    {
        [Fact]
        public void LogsTestNameWithExceptionAndStackTrace()
        {
            var testFailed = new Mock<ITestFailed>();
            testFailed.Setup(tf => tf.TestDisplayName).Returns("This is my display name \t\r\n");
            testFailed.Setup(tf => tf.Message).Returns("This is my message \t\r\n");
            testFailed.Setup(tf => tf.StackTrace).Returns("Line 1\r\nLine 2\r\nLine 3");
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null);

            visitor.OnMessage(testFailed.Object);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("ERROR: This is my display name \\t\\r\\n: This is my message \\t\\r\\n", msg),
                msg => Assert.Equal("ERROR: Line 1\r\nLine 2\r\nLine 3", msg));
        }
    }

    public class OnMessage_TestPassed
    {
        [Fact]
        public void LogsTestName()
        {
            var testPassed = new Mock<ITestPassed>();
            testPassed.Setup(tp => tp.TestDisplayName).Returns("This is my display name \t\r\n");
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null);

            visitor.OnMessage(testPassed.Object);

            Assert.Single(logger.Messages, "MESSAGE[Normal]:     This is my display name \\t\\r\\n");
        }
    }

    public class OnMessage_TestSkipped
    {
        [Fact]
        public void LogsTestNameAsWarning()
        {
            var testSkipped = new Mock<ITestSkipped>();
            testSkipped.Setup(ts => ts.TestDisplayName).Returns("This is my display name \t\r\n");
            testSkipped.Setup(ts => ts.Reason).Returns("This is my skip reason \t\r\n");
            var logger = SpyLogger.Create();
            var visitor = new StandardOutputVisitor(logger, null);

            visitor.OnMessage(testSkipped.Object);

            Assert.Single(logger.Messages, "WARNING: This is my display name \\t\\r\\n: This is my skip reason \\t\\r\\n");
        }
    }
}