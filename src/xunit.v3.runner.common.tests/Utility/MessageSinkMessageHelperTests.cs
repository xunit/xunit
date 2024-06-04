using Xunit;
using Xunit.Runner.Common;
using Xunit.v3;

public class MessageSinkMessageHelperTests
{
	public static TheoryData<_MessageSinkMessage> MessageData = new()
	{
		// Make sure this list always includes every message we serialize
		TestData.AfterTestFinished(),
		TestData.AfterTestStarting(),
		TestData.BeforeTestFinished(),
		TestData.BeforeTestStarting(),
		TestData.DiagnosticMessage(),
		TestData.DiscoveryComplete(),
		TestData.DiscoveryStarting(),
		TestData.ErrorMessage(),
		TestData.InternalDiagnosticMessage(),
		TestData.TestAssemblyCleanupFailure(),
		TestData.TestAssemblyFinished(),
		TestData.TestAssemblyStarting(),
		TestData.TestCaseCleanupFailure(),
		TestData.TestCaseDiscovered(),
		TestData.TestCaseFinished(),
		TestData.TestCaseStarting(),
		TestData.TestClassCleanupFailure(),
		TestData.TestClassConstructionFinished(),
		TestData.TestClassConstructionStarting(),
		TestData.TestClassDisposeFinished(),
		TestData.TestClassDisposeStarting(),
		TestData.TestClassFinished(),
		TestData.TestClassStarting(),
		TestData.TestCleanupFailure(),
		TestData.TestCollectionCleanupFailure(),
		TestData.TestCollectionFinished(),
		TestData.TestCollectionStarting(),
		TestData.TestFailed(warnings: ["warning 1", "warning 2"]),
		TestData.TestFinished(warnings: ["warning 1", "warning 2"]),
		TestData.TestMethodCleanupFailure(),
		TestData.TestMethodFinished(),
		TestData.TestMethodStarting(),
		TestData.TestNotRun(warnings: ["warning 1", "warning 2"]),
		TestData.TestOutput(),
		TestData.TestPassed(warnings: ["warning 1", "warning 2"]),
		TestData.TestSkipped(warnings: ["warning 1", "warning 2"]),
		TestData.TestStarting(),
	};

	[Theory(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(MessageData))]
	public void CanRoundTrip(_MessageSinkMessage message)
	{
		var serialized = message.ToJson();

		Assert.NotNull(serialized);

		var deserialized = MessageSinkMessageHelper.Deserialize(serialized);
		Assert.IsType(message.GetType(), deserialized);
		Assert.Equivalent(message, deserialized);
	}
}
