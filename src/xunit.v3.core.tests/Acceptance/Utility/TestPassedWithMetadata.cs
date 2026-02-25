using Xunit.Sdk;

public class TestPassedWithMetadata(
	IAssemblyMetadata testAssembly,
	ITestCollectionMetadata testCollection,
	ITestClassMetadata? testClass,
	ITestMethodMetadata? testMethod,
	ITestCaseMetadata testCase,
	ITestMetadata test) :
		TestResultWithMetadata(testAssembly, testCollection, testClass, testMethod, testCase, test), ITestPassed
{
	public static TestPassedWithMetadata FromTestPassed(
		ITestPassed testPassed,
		IAssemblyMetadata testAssembly,
		ITestCollectionMetadata testCollection,
		ITestClassMetadata? testClass,
		ITestMethodMetadata? testMethod,
		ITestCaseMetadata testCase,
		ITestMetadata test) =>
			new(testAssembly, testCollection, testClass, testMethod, testCase, test)
			{
				AssemblyUniqueID = testPassed.AssemblyUniqueID,
				ExecutionTime = testPassed.ExecutionTime,
				FinishTime = testPassed.FinishTime,
				Output = testPassed.Output,
				TestCaseUniqueID = testPassed.TestCaseUniqueID,
				TestClassUniqueID = testPassed.TestClassUniqueID,
				TestCollectionUniqueID = testPassed.TestCollectionUniqueID,
				TestMethodUniqueID = testPassed.TestMethodUniqueID,
				TestUniqueID = testPassed.TestUniqueID,
				Warnings = testPassed.Warnings,
			};
}
