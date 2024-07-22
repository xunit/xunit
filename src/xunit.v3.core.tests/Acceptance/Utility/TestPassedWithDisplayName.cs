using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public class TestPassedWithDisplayName : TestResultMessage, ITestResultWithDisplayName, ITestPassed
{
	public required string TestDisplayName { get; set; }

	public static TestPassedWithDisplayName FromTestPassed(
		ITestPassed testPassed,
		string testDisplayName) =>
			new()
			{
				AssemblyUniqueID = testPassed.AssemblyUniqueID,
				ExecutionTime = testPassed.ExecutionTime,
				FinishTime = testPassed.FinishTime,
				Output = testPassed.Output,
				TestCaseUniqueID = testPassed.TestCaseUniqueID,
				TestClassUniqueID = testPassed.TestClassUniqueID,
				TestCollectionUniqueID = testPassed.TestCollectionUniqueID,
				TestDisplayName = testDisplayName,
				TestMethodUniqueID = testPassed.TestMethodUniqueID,
				TestUniqueID = testPassed.TestUniqueID,
				Warnings = testPassed.Warnings,
			};

	/// <inheritdoc/>
	public override string ToString() =>
		$"{GetType().Name}({TestUniqueID.Quoted()}, {TestDisplayName.Quoted()})";
}
