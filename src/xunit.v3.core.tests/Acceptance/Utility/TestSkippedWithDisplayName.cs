using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public class TestSkippedWithDisplayName : TestResultMessage, ITestResultWithDisplayName, ITestSkipped
{
	public required string Reason { get; set; }

	public required string TestDisplayName { get; set; }

	public static TestSkippedWithDisplayName FromTestSkipped(
		ITestSkipped testSkipped,
		string testDisplayName) =>
			new()
			{
				AssemblyUniqueID = testSkipped.AssemblyUniqueID,
				ExecutionTime = testSkipped.ExecutionTime,
				FinishTime = testSkipped.FinishTime,
				Output = testSkipped.Output,
				Reason = testSkipped.Reason,
				TestCaseUniqueID = testSkipped.TestCaseUniqueID,
				TestClassUniqueID = testSkipped.TestClassUniqueID,
				TestCollectionUniqueID = testSkipped.TestCollectionUniqueID,
				TestDisplayName = testDisplayName,
				TestMethodUniqueID = testSkipped.TestMethodUniqueID,
				TestUniqueID = testSkipped.TestUniqueID,
				Warnings = testSkipped.Warnings,
			};

	/// <inheritdoc/>
	public override string ToString() =>
		$"{GetType().Name}({TestUniqueID.Quoted()}, {TestDisplayName.Quoted()})";
}
