using Xunit.Internal;
using Xunit.Sdk;

public class TestNotRunWithDisplayName : TestResultMessage, ITestResultWithDisplayName
{
	public required string TestDisplayName { get; set; }

	public static TestNotRunWithDisplayName FromTestNotRun(
		TestNotRun testNotRun,
		string testDisplayName) =>
			new()
			{
				AssemblyUniqueID = testNotRun.AssemblyUniqueID,
				ExecutionTime = testNotRun.ExecutionTime,
				Output = testNotRun.Output,
				TestCaseUniqueID = testNotRun.TestCaseUniqueID,
				TestClassUniqueID = testNotRun.TestClassUniqueID,
				TestCollectionUniqueID = testNotRun.TestCollectionUniqueID,
				TestDisplayName = testDisplayName,
				TestMethodUniqueID = testNotRun.TestMethodUniqueID,
				TestUniqueID = testNotRun.TestUniqueID,
				Warnings = testNotRun.Warnings,
			};

	/// <inheritdoc/>
	public override string ToString() =>
		$"{GetType().Name}({TestUniqueID.Quoted()}, {TestDisplayName.Quoted()})";
}
