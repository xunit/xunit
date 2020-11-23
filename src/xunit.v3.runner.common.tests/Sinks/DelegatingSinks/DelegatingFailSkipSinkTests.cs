using System.Linq;
using Xunit;
using Xunit.Runner.Common;
using Xunit.v3;

public class DelegatingFailSkipSinkTests
{
	SpyExecutionSink innerSink;
	DelegatingFailSkipSink sink;

	public DelegatingFailSkipSinkTests()
	{

		innerSink = new SpyExecutionSink();
		sink = new DelegatingFailSkipSink(innerSink);
	}

	[Fact]
	public void OnTestSkipped_TransformsToTestFailed()
	{
		var startingMessage = TestData.TestStarting();
		var skippedMessage = TestData.TestSkipped(reason: "The skip reason");

		sink.OnMessage(startingMessage);
		sink.OnMessage(skippedMessage);

		var outputMessage = Assert.Single(innerSink.Messages.OfType<_TestFailed>());
		Assert.Equal(0M, skippedMessage.ExecutionTime);
		Assert.Empty(skippedMessage.Output);
		Assert.Equal("FAIL_SKIP", outputMessage.ExceptionTypes.Single());
		Assert.Equal("The skip reason", outputMessage.Messages.Single());
		var stackTrace = Assert.Single(outputMessage.StackTraces);
		Assert.Equal("", stackTrace);
	}

	[Fact]
	public void OnTestCollectionFinished_CountsSkipsAsFails()
	{
		var inputMessage = TestData.TestCollectionFinished(testsRun: 24, testsFailed: 8, testsSkipped: 3);

		sink.OnMessage(inputMessage);

		var outputMessage = Assert.Single(innerSink.Messages.OfType<_TestCollectionFinished>());
		Assert.Equal(24, outputMessage.TestsRun);
		Assert.Equal(11, outputMessage.TestsFailed);
		Assert.Equal(0, outputMessage.TestsSkipped);
	}

	[Fact]
	public void OnTestAssemblyFinished_CountsSkipsAsFails()
	{
		var inputMessage = TestData.TestAssemblyFinished(testsRun: 24, testsFailed: 8, testsSkipped: 3);

		sink.OnMessage(inputMessage);

		var outputMessage = Assert.Single(innerSink.Messages.OfType<_TestAssemblyFinished>());
		Assert.Equal(24, outputMessage.TestsRun);
		Assert.Equal(11, outputMessage.TestsFailed);
		Assert.Equal(0, outputMessage.TestsSkipped);
	}
}
