using System.Linq;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.Common;
using Xunit.v3;

public class DelegatingFailSkipSinkTests
{
	IExecutionSink innerSink;
	DelegatingFailSkipSink sink;

	public DelegatingFailSkipSinkTests()
	{
		innerSink = Substitute.For<IExecutionSink>();
		sink = new DelegatingFailSkipSink(innerSink);
	}

	[Fact(Skip = "Re-enable this once ITestFailed is ported to _TestFailed")]
	public void OnTestSkipped_TransformsToITestFailed()
	{
		var startingMessage = TestData.TestStarting();
		var skippedMessage = TestData.TestSkipped(reason: "The skip reason");

		sink.OnMessage(startingMessage);
		sink.OnMessage(skippedMessage);

		var outputMessage = innerSink.Captured(x => x.OnMessage(null!)).Arg<ITestFailed>();
		Assert.Equal(0M, skippedMessage.ExecutionTime);
		Assert.Empty(skippedMessage.Output);
		Assert.Equal("FAIL_SKIP", outputMessage.ExceptionTypes.Single());
		Assert.Equal("The skip reason", outputMessage.Messages.Single());
		Assert.Empty(outputMessage.StackTraces.Single());
	}

	[Fact]
	public void OnITestCollectionFinished_CountsSkipsAsFails()
	{
		var inputMessage = TestData.TestCollectionFinished(testsRun: 24, testsFailed: 8, testsSkipped: 3);

		sink.OnMessage(inputMessage);

		var outputMessage = innerSink.Captured(x => x.OnMessage(null!)).Arg<_TestCollectionFinished>();
		Assert.Equal(24, outputMessage.TestsRun);
		Assert.Equal(11, outputMessage.TestsFailed);
		Assert.Equal(0, outputMessage.TestsSkipped);
	}

	[Fact]
	public void OnITestAssemblyFinished_CountsSkipsAsFails()
	{
		var inputMessage = TestData.TestAssemblyFinished(testsRun: 24, testsFailed: 8, testsSkipped: 3);

		sink.OnMessage(inputMessage);

		var outputMessage = innerSink.Captured(x => x.OnMessage(null!)).Arg<_TestAssemblyFinished>();
		Assert.Equal(24, outputMessage.TestsRun);
		Assert.Equal(11, outputMessage.TestsFailed);
		Assert.Equal(0, outputMessage.TestsSkipped);
	}
}
