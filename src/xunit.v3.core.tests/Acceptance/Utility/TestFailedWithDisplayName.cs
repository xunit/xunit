using Xunit.Internal;
using Xunit.Sdk;

public class TestFailedWithDisplayName : _TestResultMessage, ITestResultWithDisplayName
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

	public FailureCause Cause { get; set; }

	public int[] ExceptionParentIndices { get; set; }

	public string?[] ExceptionTypes { get; set; }

	public string[] Messages { get; set; }

	public string?[] StackTraces { get; set; }

	public string TestDisplayName { get; set; }

	/// <inheritdoc/>
	public override string ToString() =>
		$"{GetType().Name}({TestUniqueID.Quoted()}, {TestDisplayName.Quoted()})";
}
