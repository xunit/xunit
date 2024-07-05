using Xunit.Internal;
using Xunit.Sdk;

public class TestPassedWithDisplayName : _TestResultMessage, ITestResultWithDisplayName
{
	internal TestPassedWithDisplayName(
		_TestPassed testPassed,
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
