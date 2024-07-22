using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public class TestNotRunWithDisplayName : TestResultMessage, ITestResultWithDisplayName, ITestNotRun
{
	public required string TestDisplayName { get; set; }

	public static TestNotRunWithDisplayName FromTestNotRun(
		ITestNotRun testNotRun,
		string testDisplayName) =>
			new()
			{
				AssemblyUniqueID = testNotRun.AssemblyUniqueID,
				ExecutionTime = testNotRun.ExecutionTime,
				FinishTime = testNotRun.FinishTime,
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
