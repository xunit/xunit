using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Extension methods for <see cref="ITestCase"/>.
/// </summary>
public static class TestCaseExtensions
{
	/// <summary>
	/// Converts an instance of <see cref="ITestCase"/> into <see cref="TestCaseDiscovered"/> for reporting
	/// back to a remote meta-runner.
	/// </summary>
	public static TestCaseDiscovered ToTestCaseDiscovered(this ITestCase testCase)
	{
		Guard.ArgumentNotNull(testCase);

		return new TestCaseDiscovered()
		{
			AssemblyUniqueID = testCase.TestCollection.TestAssembly.UniqueID,
			Serialization = SerializationHelper.Serialize(testCase),
			SkipReason = testCase.SkipReason,
			SourceFilePath = testCase.SourceFilePath,
			SourceLineNumber = testCase.SourceLineNumber,
			TestCaseDisplayName = testCase.TestCaseDisplayName,
			TestCaseUniqueID = testCase.UniqueID,
			TestClassMetadataToken = testCase.TestClassMetadataToken,
			TestClassName = testCase.TestClassName,
			TestClassNamespace = testCase.TestClassNamespace,
			TestClassUniqueID = testCase.TestClass?.UniqueID,
			TestCollectionUniqueID = testCase.TestCollection.UniqueID,
			TestMethodMetadataToken = testCase.TestMethodMetadataToken,
			TestMethodName = testCase.TestMethodName,
			TestMethodUniqueID = testCase.TestMethod?.UniqueID,
			Traits = testCase.Traits,
		};
	}
}
