using Xunit.Internal;
using Xunit.v3;

public class TestFailedWithDisplayName : _TestFailed, ITestResultWithDisplayName
{
	internal TestFailedWithDisplayName(
		_TestFailed testFailed,
		string testDisplayName)
	{
		AssemblyUniqueID = testFailed.AssemblyUniqueID;
		Cause = testFailed.Cause;
		ExceptionParentIndices = testFailed.ExceptionParentIndices;
		ExceptionTypes = testFailed.ExceptionTypes;
		ExecutionTime = testFailed.ExecutionTime;
		Messages = testFailed.Messages;
		Output = testFailed.Output;
		StackTraces = testFailed.StackTraces;
		TestCaseUniqueID = testFailed.TestCaseUniqueID;
		TestClassUniqueID = testFailed.TestClassUniqueID;
		TestCollectionUniqueID = testFailed.TestCollectionUniqueID;
		TestDisplayName = testDisplayName;
		TestMethodUniqueID = testFailed.TestMethodUniqueID;
		TestUniqueID = testFailed.TestUniqueID;
		Warnings = testFailed.Warnings;
	}

	public string TestDisplayName { get; set; }

	/// <inheritdoc/>
	public override string ToString() =>
		$"{GetType().Name}({TestUniqueID.Quoted()}, {TestDisplayName.Quoted()})";
}
