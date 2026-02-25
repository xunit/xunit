using Xunit.Sdk;

public class TestSkippedWithMetadata(
	IAssemblyMetadata testAssembly,
	ITestCollectionMetadata testCollection,
	ITestClassMetadata? testClass,
	ITestMethodMetadata? testMethod,
	ITestCaseMetadata testCase,
	ITestMetadata test) :
		TestResultWithMetadata(testAssembly, testCollection, testClass, testMethod, testCase, test), ITestSkipped
{
	public required string Reason { get; set; }

	public static TestSkippedWithMetadata FromTestSkipped(
		ITestSkipped testSkipped,
		IAssemblyMetadata testAssembly,
		ITestCollectionMetadata testCollection,
		ITestClassMetadata? testClass,
		ITestMethodMetadata? testMethod,
		ITestCaseMetadata testCase,
		ITestMetadata test) =>
			new(testAssembly, testCollection, testClass, testMethod, testCase, test)
			{
				AssemblyUniqueID = testSkipped.AssemblyUniqueID,
				ExecutionTime = testSkipped.ExecutionTime,
				FinishTime = testSkipped.FinishTime,
				Output = testSkipped.Output,
				Reason = testSkipped.Reason,
				TestCaseUniqueID = testSkipped.TestCaseUniqueID,
				TestClassUniqueID = testSkipped.TestClassUniqueID,
				TestCollectionUniqueID = testSkipped.TestCollectionUniqueID,
				TestMethodUniqueID = testSkipped.TestMethodUniqueID,
				TestUniqueID = testSkipped.TestUniqueID,
				Warnings = testSkipped.Warnings,
			};
}
