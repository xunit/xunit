using System.Linq;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

public class FailSkipSinkTests
{
    IExecutionSink innerSink;
    FailSkipSink sink;

    public FailSkipSinkTests()
    {
        innerSink = Substitute.For<IExecutionSink>();
        sink = new FailSkipSink(innerSink);
    }

    [Fact]
    public void OnITestSkipped_TransformsToITestFailed()
    {
        var inputMessage = Mocks.TestSkipped("The skipped test", "The skip reason");
        var types = new[] { typeof(ITestSkipped).FullName };

        sink.OnMessageWithTypes(inputMessage, types);

        var outputMessage = innerSink.Captured(x => x.OnMessageWithTypes(null, null)).Arg<ITestFailed>();
        Assert.Equal(inputMessage.Test, outputMessage.Test);
        Assert.Equal(0M, inputMessage.ExecutionTime);
        Assert.Empty(inputMessage.Output);
        Assert.Equal("FAIL_SKIP", outputMessage.ExceptionTypes.Single());
        Assert.Equal("The skip reason", outputMessage.Messages.Single());
        Assert.Empty(outputMessage.StackTraces.Single());
    }

    [Fact]
    public void OnITestCollectionFinished_CountsSkipsAsFails()
    {
        var inputMessage = Mocks.TestCollectionFinished(testsRun: 24, testsFailed: 8, testsSkipped: 3);
        var types = new[] { typeof(ITestCollectionFinished).FullName };

        sink.OnMessageWithTypes(inputMessage, types);

        var outputMessage = innerSink.Captured(x => x.OnMessageWithTypes(null, null)).Arg<ITestCollectionFinished>();
        Assert.Equal(24, outputMessage.TestsRun);
        Assert.Equal(11, outputMessage.TestsFailed);
        Assert.Equal(0, outputMessage.TestsSkipped);
    }

    [Fact]
    public void OnITestAssemblyFinished_CountsSkipsAsFails()
    {
        var inputMessage = Mocks.TestAssemblyFinished(testsRun: 24, testsFailed: 8, testsSkipped: 3);
        var types = new[] { typeof(ITestAssemblyFinished).FullName };

        sink.OnMessageWithTypes(inputMessage, types);

        var outputMessage = innerSink.Captured(x => x.OnMessageWithTypes(null, null)).Arg<ITestAssemblyFinished>();
        Assert.Equal(24, outputMessage.TestsRun);
        Assert.Equal(11, outputMessage.TestsFailed);
        Assert.Equal(0, outputMessage.TestsSkipped);
    }
}
