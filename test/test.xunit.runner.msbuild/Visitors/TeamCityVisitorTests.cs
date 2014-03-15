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
            errorMessage.ExceptionTypes.Returns(new[] { "ExceptionType" });
            errorMessage.Messages.Returns(new[] { "This is my message \t\r\n" });
            errorMessage.StackTraces.Returns(new[] { "Line 1\r\nLine 2\r\nLine 3" });

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null);

            var result = visitor.OnMessage(errorMessage);

            Assert.Collection(logger.Messages,
                msg => Assert.Equal("ERROR: ExceptionType : This is my message \\t\\r\\n", msg),
                msg => Assert.Equal("ERROR: Line 1\r\nLine 2\r\nLine 3", msg));
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

            Assert.Single(logger.Messages, @"MESSAGE[High]: ##teamcity[testSuiteFinished name='Display Name' flowId='myFlowId']");
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
                msg => Assert.Equal(@"MESSAGE[High]: ##teamcity[testSuiteStarted name='Display Name' flowId='myFlowId']", msg));
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
            testPassed.TestDisplayName.Returns("This is my display name \t\r\n");
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
            testSkipped.TestDisplayName.Returns("This is my display name \t\r\n");
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
            testStarting.TestDisplayName.Returns("This is my display name \t\r\n");

            var logger = SpyLogger.Create();
            var visitor = new TeamCityVisitor(logger, null, null, _ => "myFlowId");

            visitor.OnMessage(testStarting);

            Assert.Single(logger.Messages, "MESSAGE[High]: ##teamcity[testStarted name='This is my display name \t|r|n' flowId='myFlowId']");
        }
    }
}