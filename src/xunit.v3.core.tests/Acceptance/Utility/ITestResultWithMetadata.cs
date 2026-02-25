using Xunit.Sdk;

public interface ITestResultWithMetadata
{
	ITestMetadata Test { get; }

	IAssemblyMetadata TestAssembly { get; }

	ITestCaseMetadata TestCase { get; }

	ITestClassMetadata? TestClass { get; }

	ITestCollectionMetadata TestCollection { get; }

	ITestMethodMetadata? TestMethod { get; }
}
