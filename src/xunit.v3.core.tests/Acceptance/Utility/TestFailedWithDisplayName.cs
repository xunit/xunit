using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public class TestFailedWithDisplayName : TestResultMessage, ITestResultWithDisplayName, ITestFailed
{
	public FailureCause Cause { get; set; }

	public required int[] ExceptionParentIndices { get; set; }

	public required string?[] ExceptionTypes { get; set; }

	public required string[] Messages { get; set; }

	public required string?[] StackTraces { get; set; }

	public required string TestDisplayName { get; set; }

	public static TestFailedWithDisplayName FromTestFailed(
		ITestFailed testFailed,
		string testDisplayName) =>
			new()
			{
				AssemblyUniqueID = testFailed.AssemblyUniqueID,
				Cause = testFailed.Cause,
				ExceptionParentIndices = testFailed.ExceptionParentIndices,
				ExceptionTypes = testFailed.ExceptionTypes,
				ExecutionTime = testFailed.ExecutionTime,
				FinishTime = testFailed.FinishTime,
				Messages = testFailed.Messages,
				Output = testFailed.Output,
				StackTraces = testFailed.StackTraces,
				TestCaseUniqueID = testFailed.TestCaseUniqueID,
				TestClassUniqueID = testFailed.TestClassUniqueID,
				TestCollectionUniqueID = testFailed.TestCollectionUniqueID,
				TestDisplayName = testDisplayName,
				TestMethodUniqueID = testFailed.TestMethodUniqueID,
				TestUniqueID = testFailed.TestUniqueID,
				Warnings = testFailed.Warnings,
			};

	/// <inheritdoc/>
	public override string ToString() =>
		$"{GetType().Name}({TestUniqueID.Quoted()}, {TestDisplayName.Quoted()})";
}
