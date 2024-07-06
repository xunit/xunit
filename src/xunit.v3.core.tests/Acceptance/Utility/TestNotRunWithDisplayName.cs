using Xunit.Internal;
using Xunit.Sdk;

public class TestNotRunWithDisplayName : TestResultMessage, ITestResultWithDisplayName
{
	internal TestNotRunWithDisplayName(
		TestNotRun testNotRun,
		string testDisplayName)
	{
		AssemblyUniqueID = testNotRun.AssemblyUniqueID;
		ExecutionTime = testNotRun.ExecutionTime;
		Output = testNotRun.Output;
		TestCaseUniqueID = testNotRun.TestCaseUniqueID;
		TestClassUniqueID = testNotRun.TestClassUniqueID;
		TestCollectionUniqueID = testNotRun.TestCollectionUniqueID;
		TestDisplayName = testDisplayName;
		TestMethodUniqueID = testNotRun.TestMethodUniqueID;
		TestUniqueID = testNotRun.TestUniqueID;
		Warnings = testNotRun.Warnings;
	}

	public string TestDisplayName { get; set; }

	/// <inheritdoc/>
	public override string ToString() =>
		$"{GetType().Name}({TestUniqueID.Quoted()}, {TestDisplayName.Quoted()})";
}
