using Xunit.Internal;
using Xunit.Sdk;

public class TestPassedWithDisplayName : TestResultMessage, ITestResultWithDisplayName
{
	internal TestPassedWithDisplayName(
		TestPassed testPassed,
		string testDisplayName)
	{
		AssemblyUniqueID = testPassed.AssemblyUniqueID;
		ExecutionTime = testPassed.ExecutionTime;
		Output = testPassed.Output;
		TestCaseUniqueID = testPassed.TestCaseUniqueID;
		TestClassUniqueID = testPassed.TestClassUniqueID;
		TestCollectionUniqueID = testPassed.TestCollectionUniqueID;
		TestDisplayName = testDisplayName;
		TestMethodUniqueID = testPassed.TestMethodUniqueID;
		TestUniqueID = testPassed.TestUniqueID;
		Warnings = testPassed.Warnings;
	}

	public string TestDisplayName { get; set; }

	/// <inheritdoc/>
	public override string ToString() =>
		$"{GetType().Name}({TestUniqueID.Quoted()}, {TestDisplayName.Quoted()})";
}
