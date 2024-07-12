using Xunit.Internal;
using Xunit.Sdk;

public class TestPassedWithDisplayName : TestResultMessage, ITestResultWithDisplayName
{
	public required string TestDisplayName { get; set; }

	public static TestPassedWithDisplayName FromTestPassed(
		TestPassed testPassed,
		string testDisplayName) =>
			new()
			{
				AssemblyUniqueID = testPassed.AssemblyUniqueID,
				ExecutionTime = testPassed.ExecutionTime,
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
