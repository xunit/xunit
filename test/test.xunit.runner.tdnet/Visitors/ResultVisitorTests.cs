using System;
using NSubstitute;
using TestDriven.Framework;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.TdNet;

public class ResultVisitorTests
{
    [Fact]
    public void SignalsFinishedEventUponReceiptOfITestAssemblyFinished()
    {
        var listener = Substitute.For<ITestListener>();
        var visitor = new ResultVisitor(listener);
        var message = Substitute.For<ITestAssemblyFinished>();

        visitor.OnMessage(message);

        Assert.True(visitor.Finished.WaitOne(0));
    }

    public class RunState
    {
        [Fact]
        public void DefaultRunStateIsNoTests()
        {
            var listener = Substitute.For<ITestListener>();
            var visitor = new ResultVisitor(listener);

            Assert.Equal(TestRunState.NoTests, visitor.TestRunState);
        }

        [Theory]
        [InlineData(TestRunState.NoTests)]
        [InlineData(TestRunState.Error)]
        [InlineData(TestRunState.Success)]
        public void FailureSetsStateToFailed(TestRunState initialState)
        {
            var listener = Substitute.For<ITestListener>();
            var visitor = new ResultVisitor(listener) { TestRunState = initialState };

            visitor.OnMessage(new TestFailed());

            Assert.Equal(TestRunState.Failure, visitor.TestRunState);
        }

        [Fact]
        public void Success_MovesToSuccess()
        {
            var listener = Substitute.For<ITestListener>();
            var visitor = new ResultVisitor(listener) { TestRunState = TestRunState.NoTests };

            visitor.OnMessage(new TestPassed());

            Assert.Equal(TestRunState.Success, visitor.TestRunState);
        }

        [Theory]
        [InlineData(TestRunState.Error)]
        [InlineData(TestRunState.Failure)]
        [InlineData(TestRunState.Success)]
        public void Success_StaysInCurrentState(TestRunState initialState)
        {
            var listener = Substitute.For<ITestListener>();
            var visitor = new ResultVisitor(listener) { TestRunState = initialState };

            visitor.OnMessage(new TestPassed());

            Assert.Equal(initialState, visitor.TestRunState);
        }

        [Fact]
        public void Skip_MovesToSuccess()
        {
            var listener = Substitute.For<ITestListener>();
            var visitor = new ResultVisitor(listener) { TestRunState = TestRunState.NoTests };

            visitor.OnMessage(new TestSkipped());

            Assert.Equal(TestRunState.Success, visitor.TestRunState);
        }

        [Theory]
        [InlineData(TestRunState.Error)]
        [InlineData(TestRunState.Failure)]
        [InlineData(TestRunState.Success)]
        public void Skip_StaysInCurrentState(TestRunState initialState)
        {
            var listener = Substitute.For<ITestListener>();
            var visitor = new ResultVisitor(listener) { TestRunState = initialState };

            visitor.OnMessage(new TestSkipped());

            Assert.Equal(initialState, visitor.TestRunState);
        }
    }

    public class MessageConversion
    {
        [Fact]
        public void ConvertsITestPassed()
        {
            TestResult testResult = null;
            var listener = Substitute.For<ITestListener>();
            listener.WhenAny(l => l.TestFinished(null))
                    .Do<TestResult>(result => testResult = result);
            var visitor = new ResultVisitor(listener);
            var message = new TestPassed
            {
                TestCase = new TestCase(typeof(string), "Contains"),
                TestDisplayName = "Display Name",
                ExecutionTime = 123.45M
            };

            visitor.OnMessage(message);

            Assert.NotNull(testResult);
            Assert.Same(typeof(string), testResult.FixtureType);
            Assert.Equal("Contains", testResult.Method.Name);
            Assert.Equal("Display Name", testResult.Name);
            Assert.Equal(TestState.Passed, testResult.State);
            Assert.Equal(123.45, testResult.TimeSpan.TotalMilliseconds);
            Assert.Equal(1, testResult.TotalTests);
        }

        [Fact]
        public void ConvertsITestFailed()
        {
            Exception ex;

            try
            {
                throw new Exception();
            }
            catch (Exception e)
            {
                ex = e;
            }

            TestResult testResult = null;
            var listener = Substitute.For<ITestListener>();
            listener.WhenAny(l => l.TestFinished(null))
                    .Do<TestResult>(result => testResult = result);
            var visitor = new ResultVisitor(listener);
            var message = new TestFailed(ex)
            {
                TestCase = new TestCase(typeof(string), "Contains"),
                TestDisplayName = "Display Name",
                ExecutionTime = 123.45M,
            };

            visitor.OnMessage(message);

            Assert.NotNull(testResult);
            Assert.Same(typeof(string), testResult.FixtureType);
            Assert.Equal("Contains", testResult.Method.Name);
            Assert.Equal("Display Name", testResult.Name);
            Assert.Equal(TestState.Failed, testResult.State);
            Assert.Equal(123.45, testResult.TimeSpan.TotalMilliseconds);
            Assert.Equal(1, testResult.TotalTests);
            Assert.Equal("System.Exception : " + ex.Message, testResult.Message);
            Assert.Equal(ex.StackTrace, testResult.StackTrace);
        }

        [Fact]
        public void ConvertsITestSkipped()
        {
            TestResult testResult = null;
            var listener = Substitute.For<ITestListener>();
            listener.WhenAny(l => l.TestFinished(null))
                    .Do<TestResult>(result => testResult = result);
            var visitor = new ResultVisitor(listener);
            var message = new TestSkipped
            {
                TestCase = new TestCase(typeof(string), "Contains"),
                TestDisplayName = "Display Name",
                ExecutionTime = 123.45M,
                Reason = "I forgot how to run"
            };

            visitor.OnMessage(message);

            Assert.NotNull(testResult);
            Assert.Same(typeof(string), testResult.FixtureType);
            Assert.Equal("Contains", testResult.Method.Name);
            Assert.Equal("Display Name", testResult.Name);
            Assert.Equal(TestState.Ignored, testResult.State);
            Assert.Equal(123.45, testResult.TimeSpan.TotalMilliseconds);
            Assert.Equal(1, testResult.TotalTests);
            Assert.Equal("I forgot how to run", testResult.Message);
        }
    }
}