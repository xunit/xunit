using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.MSBuild;

public class TeamCityVisitorTests
{
    public class OnMessage_ErrorMessage
    {
        [Fact]
        public void LogsMessage()
        {
            var errorMessage = Substitute.For<IErrorMessage>();
            errorMessage.ExceptionType.Returns("ExceptionType");
            errorMessage.Message.Returns("This is my message \t\r\n");
            errorMessage.StackTrace.Returns("Line 1\r\nLine 2\r\nLine 3");

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null, @"C:\Foo\Bar.dll");

            var result = visitor.OnMessage(errorMessage);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("ERROR: ExceptionType: This is my message \\t\\r\\n", msg),
                msg => Assert.Equal("ERROR: Line 1\r\nLine 2\r\nLine 3", msg));
        }
    }

    public class OnMessage_TestAssemblyFinished
    {
        [Fact]
        public void LogsMessage()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            assemblyFinished.TestsRun.Returns(2112);
            assemblyFinished.TestsFailed.Returns(42);
            assemblyFinished.TestsSkipped.Returns(6);
            assemblyFinished.ExecutionTime.Returns(123.4567M);

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null, @"C:\Foo\Bar.dll") { FlowId = "myFlowId" };

            visitor.OnMessage(assemblyFinished);

            Assert.Single(logger.Messages, @"MESSAGE[High]: ##teamcity[testSuiteFinished name='C:\Foo\Bar.dll' flowId='myFlowId']");
        }
    }

    public class OnMessage_TestAssemblyStarting
    {
        [Fact]
        public void LogsMessage()
        {
            var assemblyStarting = Substitute.For<ITestAssemblyStarting>();

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null, @"C:\Foo\Bar.dll") { FlowId = "myFlowId" };

            visitor.OnMessage(assemblyStarting);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal(@"MESSAGE[High]: ##teamcity[testSuiteStarted name='C:\Foo\Bar.dll' flowId='myFlowId']", msg));
        }
    }

    public class OnMessage_TestFailed
    {
        [Fact]
        public void LogsTestNameWithExceptionAndStackTrace()
        {
            var testFailed = Substitute.For<ITestFailed>();
            testFailed.TestDisplayName.Returns("This is my display name \t\r\n");
            testFailed.ExecutionTime.Returns(1.2345M);
            testFailed.Message.Returns("This is my message \t\r\n");
            testFailed.StackTrace.Returns("Line 1\r\nLine 2\r\nLine 3");

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null, @"C:\Foo\Bar.dll") { FlowId = "myFlowId" };

            visitor.OnMessage(testFailed);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("MESSAGE[High]: ##teamcity[testFailed name='This is my display name \t|r|n' details='This is my message \t|r|n|r|nLine 1|r|nLine 2|r|nLine 3' flowId='myFlowId']", msg),
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
            testPassed.TestDisplayName.Returns("This is my display name \t\r\n");
            testPassed.ExecutionTime.Returns(1.2345M);

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null, @"C:\Foo\Bar.dll") { FlowId = "myFlowId" };

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
            testSkipped.TestDisplayName.Returns("This is my display name \t\r\n");
            testSkipped.Reason.Returns("This is my skip reason \t\r\n");

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null, @"C:\Foo\Bar.dll") { FlowId = "myFlowId" };

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
            testStarting.TestDisplayName.Returns("This is my display name \t\r\n");

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null, @"C:\Foo\Bar.dll") { FlowId = "myFlowId" };

            visitor.OnMessage(testStarting);

            Assert.Single(logger.Messages, "MESSAGE[High]: ##teamcity[testStarted name='This is my display name \t|r|n' flowId='myFlowId']");
        }
    }
}