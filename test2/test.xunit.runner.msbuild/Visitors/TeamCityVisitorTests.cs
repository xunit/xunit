using Moq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.MSBuild;

public class TeamCityVisitorTests
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
            var visitor = new TeamCityVisitor(logger, null, @"C:\Foo\Bar.dll");

            var result = visitor.OnMessage(errorMessage);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("ERROR: ExceptionType: This is my message \\t\\r\\n", msg),
                msg => Assert.Equal("ERROR: Line 1\r\nLine 2\r\nLine 3", msg));
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, () => true, @"C:\Foo\Bar.dll");

            var result = visitor.OnMessage(errorMessage);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, () => false, @"C:\Foo\Bar.dll");

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
        public void LogsMessage()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, @"C:\Foo\Bar.dll");

            visitor.OnMessage(assemblyFinished);

            Assert.Single(logger.Messages, @"MESSAGE[High]: ##teamcity[testSuiteFinished name='C:\Foo\Bar.dll']");
        }

        [Fact]
        public void AddsStatisticsToRunningTotal()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, @"C:\Foo\Bar.dll") { Total = 10, Failed = 10, Skipped = 10, Time = 10M };

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
            var visitor = new TeamCityVisitor(logger, () => true, @"C:\Foo\Bar.dll");

            var result = visitor.OnMessage(assemblyFinished);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, () => false, @"C:\Foo\Bar.dll");

            var result = visitor.OnMessage(assemblyFinished);

            Assert.True(result);
        }
    }

    public class OnMessage_TestAssemblyStarting
    {
        ITestAssemblyStarting assemblyStarting;

        public OnMessage_TestAssemblyStarting()
        {
            var mockMessage = new Mock<ITestAssemblyStarting>();
            assemblyStarting = mockMessage.Object;
        }

        [Fact]
        public void LogsMessage()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, @"C:\Foo\Bar.dll");

            visitor.OnMessage(assemblyStarting);

            Assert.Single(logger.Messages, @"MESSAGE[High]: ##teamcity[testSuiteStarted name='C:\Foo\Bar.dll']");
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, () => true, @"C:\Foo\Bar.dll");

            var result = visitor.OnMessage(assemblyStarting);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, () => false, @"C:\Foo\Bar.dll");

            var result = visitor.OnMessage(assemblyStarting);

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
            mockMessage.Setup(tf => tf.ExecutionTime).Returns(1.2345M);
            mockMessage.Setup(tf => tf.Message).Returns("This is my message \t\r\n");
            mockMessage.Setup(tf => tf.StackTrace).Returns("Line 1\r\nLine 2\r\nLine 3");
            testFailed = mockMessage.Object;
        }

        [Fact]
        public void LogsTestNameWithExceptionAndStackTrace()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, @"C:\Foo\Bar.dll");

            visitor.OnMessage(testFailed);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testFailed name='This is my display name \t|r|n' details='This is my message \t|r|n|r|nLine 1|r|nLine 2|r|nLine 3']", msg),
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testFinished name='This is my display name \t|r|n' duration='1234']", msg)
            );
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, () => true, @"C:\Foo\Bar.dll");

            var result = visitor.OnMessage(testFailed);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, () => false, @"C:\Foo\Bar.dll");

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
            mockMessage.Setup(tp => tp.ExecutionTime).Returns(1.2345M);
            testPassed = mockMessage.Object;
        }

        [Fact]
        public void LogsTestName()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, @"C:\Foo\Bar.dll");

            visitor.OnMessage(testPassed);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testFinished name='This is my display name \t|r|n' duration='1234']", msg)
            );
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, () => true, @"C:\Foo\Bar.dll");

            var result = visitor.OnMessage(testPassed);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, () => false, @"C:\Foo\Bar.dll");

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
            var visitor = new TeamCityVisitor(logger, null, @"C:\Foo\Bar.dll");

            visitor.OnMessage(testSkipped);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testIgnored name='This is my display name \t|r|n' message='This is my skip reason \t|r|n']", msg),
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testFinished name='This is my display name \t|r|n' duration='0']", msg)
            );
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, () => true, @"C:\Foo\Bar.dll");

            var result = visitor.OnMessage(testSkipped);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, () => false, @"C:\Foo\Bar.dll");

            var result = visitor.OnMessage(testSkipped);

            Assert.True(result);
        }
    }

    public class OnMessage_TestStarting
    {
        ITestStarting testStarting;

        public OnMessage_TestStarting()
        {
            var mockMessage = new Mock<ITestStarting>();
            mockMessage.Setup(tp => tp.TestDisplayName).Returns("This is my display name \t\r\n");
            testStarting = mockMessage.Object;
        }

        [Fact]
        public void LogsTestName()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, @"C:\Foo\Bar.dll");

            visitor.OnMessage(testStarting);

            Assert.Single(logger.Messages, "MESSAGE[High]: ##teamcity[testStarted name='This is my display name \t|r|n']");
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, () => true, @"C:\Foo\Bar.dll");

            var result = visitor.OnMessage(testStarting);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, () => false, @"C:\Foo\Bar.dll");

            var result = visitor.OnMessage(testStarting);

            Assert.True(result);
        }
    }
}