using Xunit.Sdk;

public class TestNotRunWithMetadata(
	IAssemblyMetadata testAssembly,
	ITestCollectionMetadata testCollection,
	ITestClassMetadata? testClass,
	ITestMethodMetadata? testMethod,
	ITestCaseMetadata testCase,
	ITestMetadata test) :
		TestResultWithMetadata(testAssembly, testCollection, testClass, testMethod, testCase, test), ITestNotRun
{
	public static TestNotRunWithMetadata FromTestNotRun(
		ITestNotRun testNotRun,
		IAssemblyMetadata testAssembly,
		ITestCollectionMetadata testCollection,
		ITestClassMetadata? testClass,
		ITestMethodMetadata? testMethod,
		ITestCaseMetadata testCase,
		ITestMetadata test) =>
			new(testAssembly, testCollection, testClass, testMethod, testCase, test)
			{
				AssemblyUniqueID = testNotRun.AssemblyUniqueID,
				ExecutionTime = testNotRun.ExecutionTime,
				FinishTime = testNotRun.FinishTime,
				Output = testNotRun.Output,
				TestCaseUniqueID = testNotRun.TestCaseUniqueID,
				TestClassUniqueID = testNotRun.TestClassUniqueID,
				TestCollectionUniqueID = testNotRun.TestCollectionUniqueID,
				TestMethodUniqueID = testNotRun.TestMethodUniqueID,
				TestUniqueID = testNotRun.TestUniqueID,
				Warnings = testNotRun.Warnings,
			};
}
