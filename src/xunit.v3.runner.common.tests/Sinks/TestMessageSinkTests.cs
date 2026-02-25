using Xunit;
using Xunit.Sdk;

public class TestMessageSinkTests
{
	public static IEnumerable<TheoryDataRow<IMessageSinkMessage>> MessageData =
	[
		// Diagnostics
		new(TestData.DiagnosticMessage()),
		new(TestData.ErrorMessage()),

		// Discovery
		new(TestData.DiscoveryComplete()),
		new(TestData.DiscoveryStarting()),
		new(TestData.TestCaseDiscovered()),

		// Execution
		new(TestData.AfterTestFinished()),
		new(TestData.AfterTestStarting()),
		new(TestData.BeforeTestFinished()),
		new(TestData.BeforeTestStarting()),
		new(TestData.TestAssemblyCleanupFailure()),
		new(TestData.TestAssemblyFinished()),
		new(TestData.TestAssemblyStarting()),
		new(TestData.TestCaseCleanupFailure()),
		new(TestData.TestCaseFinished()),
		new(TestData.TestCaseStarting()),
		new(TestData.TestClassCleanupFailure()),
		new(TestData.TestClassConstructionFinished()),
		new(TestData.TestClassConstructionStarting()),
		new(TestData.TestClassDisposeFinished()),
		new(TestData.TestClassDisposeStarting()),
		new(TestData.TestClassFinished()),
		new(TestData.TestClassStarting()),
		new(TestData.TestCollectionCleanupFailure()),
		new(TestData.TestCollectionFinished()),
		new(TestData.TestCollectionStarting()),
		new(TestData.TestCleanupFailure()),
		new(TestData.TestFailed()),
		new(TestData.TestFinished()),
		new(TestData.TestMethodCleanupFailure()),
		new(TestData.TestMethodFinished()),
		new(TestData.TestMethodStarting()),
		new(TestData.TestNotRun()),
		new(TestData.TestOutput()),
		new(TestData.TestPassed()),
		new(TestData.TestSkipped()),
		new(TestData.TestStarting()),

		// Runner
		TestData.TestAssemblyExecutionStarting(),
		TestData.TestAssemblyExecutionFinished(),
		TestData.TestAssemblyDiscoveryStarting(),
		TestData.TestAssemblyDiscoveryFinished(),
		TestData.TestExecutionSummaries(),
	];

	[Theory(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(MessageData))]
	public static void ProcessesVisitorTypes(IMessageSinkMessage message)
	{
		var typedMessage = Assert.IsType<IMessageSinkMessage>(message, exactMatch: false);
		var sink = new SpyTestMessageSink();

		sink.OnMessage(typedMessage);

		var msg = Assert.Single(sink.Calls);
		Assert.Equal(message.GetType().Name, msg);
	}
}
