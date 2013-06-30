using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.MSBuild;

public class MSBuildVisitorTests
{
    public class OnMessage
    {
        ITestMessage testMessage;

        public OnMessage()
        {
            testMessage = Substitute.For<ITestMessage>();
        }

        [Fact]
        public void ReturnsFalseWhenCancellationThunkIsTrue()
        {
            var logger = SpyLogger.Create();
            var visitor = new MSBuildVisitor(logger, () => true);

            var result = visitor.OnMessage(testMessage);

            Assert.False(result);
        }

        [Fact]
        public void ReturnsTrueWhenCancellationThunkIsFalse()
        {
            var logger = SpyLogger.Create();
            var visitor = new MSBuildVisitor(logger, () => false);

            var result = visitor.OnMessage(testMessage);

            Assert.True(result);
        }
    }

    public class OnMessage_TestAssemblyFinished
    {
        [Fact]
        public void AddsStatisticsToRunningTotal()
        {
            var assemblyFinished = Substitute.For<ITestAssemblyFinished>();
            assemblyFinished.TestsRun.Returns(2112);
            assemblyFinished.TestsFailed.Returns(42);
            assemblyFinished.TestsSkipped.Returns(6);
            assemblyFinished.ExecutionTime.Returns(123.4567M);

            var logger = SpyLogger.Create();
            var visitor = new MSBuildVisitor(logger, () => false) { Total = 10, Failed = 10, Skipped = 10, Time = 10M };

            visitor.OnMessage(assemblyFinished);

            Assert.Equal(2122, visitor.Total);
            Assert.Equal(52, visitor.Failed);
            Assert.Equal(16, visitor.Skipped);
            Assert.Equal(133.4567M, visitor.Time);
        }
    }
}