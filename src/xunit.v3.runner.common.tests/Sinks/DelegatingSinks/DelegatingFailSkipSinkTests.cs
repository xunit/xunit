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

	public static TheoryData<_MessageSinkMessage> FinishedMessages = new()
	{
		TestData.TestCaseFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
		TestData.TestMethodFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
		TestData.TestClassFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
		TestData.TestCollectionFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
		TestData.TestAssemblyFinished(testsTotal: 24, testsFailed: 8, testsSkipped: 3, testsNotRun: 1),
	};

	[Theory(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(FinishedMessages))]
	public void OnTestCaseFinished_CountsSkipsAsFails(_MessageSinkMessage finishedMessage)
	{
		var inputSummary = (_IExecutionSummaryMetadata)finishedMessage;

		sink.OnMessage(finishedMessage);

		var outputSummary = Assert.Single(innerSink.Messages.OfType<_IExecutionSummaryMetadata>());
		Assert.Equal(inputSummary.TestsTotal, outputSummary.TestsTotal);
		Assert.Equal(inputSummary.TestsFailed + inputSummary.TestsSkipped, outputSummary.TestsFailed);
		Assert.Equal(inputSummary.TestsNotRun, outputSummary.TestsNotRun);
		Assert.Equal(0, outputSummary.TestsSkipped);
	}
}
