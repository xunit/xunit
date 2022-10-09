using System.Linq;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class DelegatingFailSkipSinkTests
{
	readonly SpyExecutionSink innerSink;
	readonly DelegatingFailSkipSink sink;

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
		Assert.Equal(FailureCause.Other, outputMessage.Cause);
		Assert.Equal(0M, outputMessage.ExecutionTime);
		Assert.Empty(outputMessage.Output);
		Assert.Equal("FAIL_SKIP", outputMessage.ExceptionTypes.Single());
		Assert.Equal("The skip reason", outputMessage.Messages.Single());
		var stackTrace = Assert.Single(outputMessage.StackTraces);
		Assert.Equal("", stackTrace);
	}

	[Fact]
	public void OnTestCaseFinished_CountsSkipsAsFails()
	{
		var inputMessage = TestData.TestCaseFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1);

		sink.OnMessage(inputMessage);

		var outputMessage = Assert.Single(innerSink.Messages.OfType<_TestCaseFinished>());
		Assert.Equal(24, outputMessage.TestsTotal);
		Assert.Equal(11, outputMessage.TestsFailed);
		Assert.Equal(1, outputMessage.TestsNotRun);
		Assert.Equal(0, outputMessage.TestsSkipped);
	}

	[Fact]
	public void OnTestMethodFinished_CountsSkipsAsFails()
	{
		var inputMessage = TestData.TestMethodFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1);

		sink.OnMessage(inputMessage);

		var outputMessage = Assert.Single(innerSink.Messages.OfType<_TestMethodFinished>());
		Assert.Equal(24, outputMessage.TestsTotal);
		Assert.Equal(11, outputMessage.TestsFailed);
		Assert.Equal(1, outputMessage.TestsNotRun);
		Assert.Equal(0, outputMessage.TestsSkipped);
	}

	[Fact]
	public void OnTestClassFinished_CountsSkipsAsFails()
	{
		var inputMessage = TestData.TestClassFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1);

		sink.OnMessage(inputMessage);

		var outputMessage = Assert.Single(innerSink.Messages.OfType<_TestClassFinished>());
		Assert.Equal(24, outputMessage.TestsTotal);
		Assert.Equal(11, outputMessage.TestsFailed);
		Assert.Equal(1, outputMessage.TestsNotRun);
		Assert.Equal(0, outputMessage.TestsSkipped);
	}

	[Fact]
	public void OnTestCollectionFinished_CountsSkipsAsFails()
	{
		var inputMessage = TestData.TestCollectionFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1);

		sink.OnMessage(inputMessage);

		var outputMessage = Assert.Single(innerSink.Messages.OfType<_TestCollectionFinished>());
		Assert.Equal(24, outputMessage.TestsTotal);
		Assert.Equal(11, outputMessage.TestsFailed);
		Assert.Equal(1, outputMessage.TestsNotRun);
		Assert.Equal(0, outputMessage.TestsSkipped);
	}

	[Fact]
	public void OnTestAssemblyFinished_CountsSkipsAsFails()
	{
		var inputMessage = TestData.TestAssemblyFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1);

		sink.OnMessage(inputMessage);

		var outputMessage = Assert.Single(innerSink.Messages.OfType<_TestAssemblyFinished>());
		Assert.Equal(24, outputMessage.TestsTotal);
		Assert.Equal(11, outputMessage.TestsFailed);
		Assert.Equal(1, outputMessage.TestsNotRun);
		Assert.Equal(0, outputMessage.TestsSkipped);
	}
}
