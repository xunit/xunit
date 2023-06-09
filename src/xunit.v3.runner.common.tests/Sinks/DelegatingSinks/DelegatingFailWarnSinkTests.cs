using System.Linq;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class DelegatingFailWarnSinkTests
{
	readonly SpyExecutionSink innerSink;
	readonly DelegatingFailWarnSink sink;

	public DelegatingFailWarnSinkTests()
	{
		innerSink = new SpyExecutionSink();
		sink = new DelegatingFailWarnSink(innerSink);
	}

	[Fact]
	public void OnTestPassed_WithWarnings_TransformsToTestFailed()
	{
		var startingMessage = TestData.TestStarting();
		var passedMessage = TestData.TestPassed(warnings: new[] { "warning" });

		sink.OnMessage(startingMessage);
		sink.OnMessage(passedMessage);

		var outputMessage = Assert.Single(innerSink.Messages.OfType<_TestFailed>());
		Assert.Equal(FailureCause.Other, outputMessage.Cause);
		Assert.Equal("FAIL_WARN", outputMessage.ExceptionTypes.Single());
		Assert.Equal("This test failed due to one or more warnings", outputMessage.Messages.Single());
		var stackTrace = Assert.Single(outputMessage.StackTraces);
		Assert.Equal("", stackTrace);
		Assert.NotNull(outputMessage.Warnings);
		var warning = Assert.Single(outputMessage.Warnings);
		Assert.Equal("warning", warning);
	}

	public static TheoryData<_TestResultMessage> OtherWarningMessages = new()
	{
		TestData.TestPassed(warnings: null),
		TestData.TestFailed(warnings: null),
		TestData.TestFailed(warnings: new[] { "warning" }),
		TestData.TestSkipped(warnings: null),
		TestData.TestSkipped(warnings: new[] { "warning" }),
		TestData.TestNotRun(warnings: null),
		TestData.TestNotRun(warnings: new[] { "warning" }),
	};

	[Theory(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(OtherWarningMessages))]
	public void OtherResultMessages_PassesThrough(_TestResultMessage inputResult)
	{
		var startingMessage = TestData.TestStarting();

		sink.OnMessage(startingMessage);
		sink.OnMessage(inputResult);

		var outputResult = Assert.Single(innerSink.Messages.OfType<_TestResultMessage>());
		Assert.Same(inputResult, outputResult);
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
	public void CountsWarnsAsFails(_MessageSinkMessage finishedMessage)
	{
		var startingMessage = TestData.TestStarting();
		var passedMessage = TestData.TestPassed(warnings: new[] { "warning" });
		var inputSummary = (_IExecutionSummaryMetadata)finishedMessage;

		sink.OnMessage(startingMessage);
		sink.OnMessage(passedMessage);
		sink.OnMessage(finishedMessage);

		var outputSummary = Assert.Single(innerSink.Messages.OfType<_IExecutionSummaryMetadata>());
		Assert.Equal(inputSummary.TestsTotal, outputSummary.TestsTotal);
		Assert.Equal(inputSummary.TestsFailed + 1, outputSummary.TestsFailed);
		Assert.Equal(inputSummary.TestsNotRun, outputSummary.TestsNotRun);
		Assert.Equal(inputSummary.TestsSkipped, outputSummary.TestsSkipped);
	}
}
