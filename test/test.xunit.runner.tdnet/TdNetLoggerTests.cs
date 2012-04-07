using System;
using System.Reflection;
using TestDriven.Framework;
using Xunit;
using Xunit.Runner.TdNet;
using TestResult = TestDriven.Framework.TestResult;

public class TdNetLoggerTests
{
    [Fact]
    public void ClassFailed()
    {
        TestableTdNetLogger logger = TestableTdNetLogger.Create();

        bool result = logger.ClassFailed("TdNetLoggerTests", null, "Message", "StackTrace");

        TestResult summary = logger.Listener.TestFinished_Summaries[0];
        Assert.Equal(typeof(TdNetLoggerTests), summary.FixtureType);
        Assert.Equal("Fixture TdNetLoggerTests", summary.Name);
        Assert.Equal(1, summary.TotalTests);
        Assert.Equal(TestState.Failed, summary.State);
        Assert.Equal("Message", summary.Message);
        Assert.Equal("StackTrace", summary.StackTrace);
        Assert.True(result);
    }

    [Fact]
    public void ExceptionThrown()
    {
        TestableTdNetLogger logger = TestableTdNetLogger.Create();
        Exception ex = new Exception("message");

        logger.ExceptionThrown(null, ex);

        Assert.Equal(ex.ToString(), logger.Listener.WriteLine__Lines[0].Key);
        Assert.Equal(Category.Error, logger.Listener.WriteLine__Lines[0].Value);
    }

    [Fact]
    public void TestFailed()
    {
        TestableTdNetLogger logger = TestableTdNetLogger.Create();

        logger.TestFailed("name", "TdNetLoggerTests", "TestFailed", 1.234, null, null, "Message", "StackTrace");

        TestResult summary = logger.Listener.TestFinished_Summaries[0];
        Assert.Equal(typeof(TdNetLoggerTests), summary.FixtureType);
        Assert.Equal(typeof(TdNetLoggerTests).GetMethod("TestFailed"), summary.Method);
        Assert.Equal("name", summary.Name);
        Assert.Equal(1, summary.TotalTests);
        Assert.Equal(TestState.Failed, summary.State);
        Assert.Equal(new TimeSpan(12340), summary.TimeSpan);
        Assert.Equal("Message", summary.Message);
        Assert.Equal("StackTrace", summary.StackTrace);
    }

    [Fact]
    public void TestFailedWithOutput()
    {
        TestableTdNetLogger logger = TestableTdNetLogger.Create();

        logger.TestFailed("TestName", "TdNetLoggerTests", "TestFailedWithOutput", 1.234, "This is" + Environment.NewLine + "output", null, null, null);

        Assert.Equal("Output from TestName:", logger.Listener.WriteLine__Lines[0].Key);
        Assert.Equal(Category.Output, logger.Listener.WriteLine__Lines[0].Value);
        Assert.Equal("  This is", logger.Listener.WriteLine__Lines[1].Key);
        Assert.Equal(Category.Output, logger.Listener.WriteLine__Lines[1].Value);
        Assert.Equal("  output", logger.Listener.WriteLine__Lines[2].Key);
        Assert.Equal(Category.Output, logger.Listener.WriteLine__Lines[2].Value);
    }

    [Fact]
    public void TestFinished()
    {
        TestableTdNetLogger logger = TestableTdNetLogger.Create();

        bool result = logger.TestFinished(null, null, null);

        Assert.True(result);
        Assert.True(logger.FoundTests);
    }

    [Fact]
    public void TestPassed()
    {
        TestableTdNetLogger logger = TestableTdNetLogger.Create();

        logger.TestPassed("name", "TdNetLoggerTests", "TestPassed", 1.234, null);

        TestResult summary = logger.Listener.TestFinished_Summaries[0];
        Assert.Equal(typeof(TdNetLoggerTests), summary.FixtureType);
        Assert.Equal(typeof(TdNetLoggerTests).GetMethod("TestPassed"), summary.Method);
        Assert.Equal("name", summary.Name);
        Assert.Equal(1, summary.TotalTests);
        Assert.Equal(TestState.Passed, summary.State);
        Assert.Equal(new TimeSpan(12340), summary.TimeSpan);
        Assert.Null(summary.Message);
        Assert.Null(summary.StackTrace);
    }

    [Fact]
    public void TestPassedWithOutput()
    {
        TestableTdNetLogger logger = TestableTdNetLogger.Create();

        logger.TestPassed("TestName", "TdNetLoggerTests", "TestPassedWithOutput", 1.234, "This is" + Environment.NewLine + "output");

        Assert.Equal("Output from TestName:", logger.Listener.WriteLine__Lines[0].Key);
        Assert.Equal(Category.Output, logger.Listener.WriteLine__Lines[0].Value);
        Assert.Equal("  This is", logger.Listener.WriteLine__Lines[1].Key);
        Assert.Equal(Category.Output, logger.Listener.WriteLine__Lines[1].Value);
        Assert.Equal("  output", logger.Listener.WriteLine__Lines[2].Key);
        Assert.Equal(Category.Output, logger.Listener.WriteLine__Lines[2].Value);
    }

    [Fact]
    public void TestSkipped()
    {
        TestableTdNetLogger logger = TestableTdNetLogger.Create();

        logger.TestSkipped("name", "TdNetLoggerTests", "TestSkipped", "reason");

        TestResult summary = logger.Listener.TestFinished_Summaries[0];
        Assert.Equal(typeof(TdNetLoggerTests), summary.FixtureType);
        Assert.Equal(typeof(TdNetLoggerTests).GetMethod("TestSkipped"), summary.Method);
        Assert.Equal("name", summary.Name);
        Assert.Equal(1, summary.TotalTests);
        Assert.Equal(TestState.Ignored, summary.State);
        Assert.Equal(new TimeSpan(0), summary.TimeSpan);
        Assert.Equal("reason", summary.Message);
        Assert.Null(summary.StackTrace);
    }

    class TestableTdNetLogger : TdNetLogger
    {
        public readonly StubTestListener Listener;

        TestableTdNetLogger(StubTestListener listener)
            : base(listener, Assembly.GetExecutingAssembly())
        {
            Listener = listener;
        }

        public static TestableTdNetLogger Create()
        {
            return new TestableTdNetLogger(new StubTestListener());
        }
    }
}