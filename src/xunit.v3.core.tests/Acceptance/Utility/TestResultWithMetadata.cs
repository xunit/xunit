using Xunit.Sdk;
using Xunit.v3;

public class TestResultWithMetadata(
	IAssemblyMetadata testAssembly,
	ITestCollectionMetadata testCollection,
	ITestClassMetadata? testClass,
	ITestMethodMetadata? testMethod,
	ITestCaseMetadata testCase,
	ITestMetadata test) :
		TestResultMessage, ITestResultWithMetadata
{
	public ITestMetadata Test =>
		test;

	public IAssemblyMetadata TestAssembly =>
		testAssembly;

	public ITestCaseMetadata TestCase =>
		testCase;

	public ITestClassMetadata? TestClass =>
		testClass;

	public ITestCollectionMetadata TestCollection =>
		testCollection;

	public ITestMethodMetadata? TestMethod =>
		testMethod;

	/// <inheritdoc/>
	public override string ToString() =>
		$"{GetType().Name}({TestUniqueID.Quoted()}, {Test.TestDisplayName.Quoted()})";
}
