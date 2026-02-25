using Xunit.Sdk;

public class TestFailedWithMetadata(
	IAssemblyMetadata testAssembly,
	ITestCollectionMetadata testCollection,
	ITestClassMetadata? testClass,
	ITestMethodMetadata? testMethod,
	ITestCaseMetadata testCase,
	ITestMetadata test) :
		TestResultWithMetadata(testAssembly, testCollection, testClass, testMethod, testCase, test), ITestFailed
{
	public FailureCause Cause { get; set; }

	public required int[] ExceptionParentIndices { get; set; }

	public required string?[] ExceptionTypes { get; set; }

	public required string[] Messages { get; set; }

	public required string?[] StackTraces { get; set; }

	public static TestFailedWithMetadata FromTestFailed(
		ITestFailed testFailed,
		IAssemblyMetadata testAssembly,
		ITestCollectionMetadata testCollection,
		ITestClassMetadata? testClass,
		ITestMethodMetadata? testMethod,
		ITestCaseMetadata testCase,
		ITestMetadata test) =>
			new(testAssembly, testCollection, testClass, testMethod, testCase, test)
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
				TestMethodUniqueID = testFailed.TestMethodUniqueID,
				TestUniqueID = testFailed.TestUniqueID,
				Warnings = testFailed.Warnings,
			};
}
