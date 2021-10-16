using Xunit.v3;

public class TestPassedWithDisplayName : _TestPassed, ITestResultWithDisplayName
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
	}

	public string TestDisplayName { get; set; }
}
