using NSubstitute;
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
            errorMessage = Substitute.For<IErrorMessage>();
            errorMessage.ExceptionType.Returns("ExceptionType");
            errorMessage.Message.Returns("This is my message \t\r\n");
            errorMessage.StackTrace.Returns("Line 1\r\nLine 2\r\nLine 3");
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
            assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            assemblyFinished.TestsRun.Returns(2112);
            assemblyFinished.TestsFailed.Returns(42);
            assemblyFinished.TestsSkipped.Returns(6);
            assemblyFinished.ExecutionTime.Returns(123.4567M);
        }

        [Fact]
        public void LogsMessage()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, @"C:\Foo\Bar.dll") { FlowId = "myFlowId" };

            visitor.OnMessage(assemblyFinished);

            Assert.Single(logger.Messages, @"MESSAGE[High]: ##teamcity[testSuiteFinished name='C:\Foo\Bar.dll' flowId='myFlowId']");
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
            assemblyStarting = Substitute.For<ITestAssemblyStarting>();
        }

        [Fact]
        public void LogsMessage()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, @"C:\Foo\Bar.dll") { FlowId = "myFlowId" };

            visitor.OnMessage(assemblyStarting);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal(@"MESSAGE[High]: ##teamcity[testSuiteStarted name='C:\Foo\Bar.dll' flowId='myFlowId']", msg));
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
            testFailed = Substitute.For<ITestFailed>();
            testFailed.TestDisplayName.Returns("This is my display name \t\r\n");
            testFailed.ExecutionTime.Returns(1.2345M);
            testFailed.Message.Returns("This is my message \t\r\n");
            testFailed.StackTrace.Returns("Line 1\r\nLine 2\r\nLine 3");
        }

        [Fact]
        public void LogsTestNameWithExceptionAndStackTrace()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, @"C:\Foo\Bar.dll") { FlowId = "myFlowId" };

            visitor.OnMessage(testFailed);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testFailed name='This is my display name \t|r|n' details='This is my message \t|r|n|r|nLine 1|r|nLine 2|r|nLine 3' flowId='myFlowId']", msg),
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testFinished name='This is my display name \t|r|n' duration='1234' flowId='myFlowId']", msg)
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
            testPassed = Substitute.For<ITestPassed>();
            testPassed.TestDisplayName.Returns("This is my display name \t\r\n");
            testPassed.ExecutionTime.Returns(1.2345M);
        }

        [Fact]
        public void LogsTestName()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, @"C:\Foo\Bar.dll") { FlowId = "myFlowId" };

            visitor.OnMessage(testPassed);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testFinished name='This is my display name \t|r|n' duration='1234' flowId='myFlowId']", msg)
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
            testSkipped = Substitute.For<ITestSkipped>();
            testSkipped.TestDisplayName.Returns("This is my display name \t\r\n");
            testSkipped.Reason.Returns("This is my skip reason \t\r\n");
        }

        [Fact]
        public void LogsTestNameAsWarning()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, @"C:\Foo\Bar.dll") { FlowId = "myFlowId" };

            visitor.OnMessage(testSkipped);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testIgnored name='This is my display name \t|r|n' message='This is my skip reason \t|r|n' flowId='myFlowId']", msg),
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testFinished name='This is my display name \t|r|n' duration='0' flowId='myFlowId']", msg)
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
            testStarting = Substitute.For<ITestStarting>();
            testStarting.TestDisplayName.Returns("This is my display name \t\r\n");
        }

        [Fact]
        public void LogsTestName()
        {
            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, @"C:\Foo\Bar.dll") { FlowId = "myFlowId" };

            visitor.OnMessage(testStarting);

            Assert.Single(logger.Messages, "MESSAGE[High]: ##teamcity[testStarted name='This is my display name \t|r|n' flowId='myFlowId']");
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