using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Extension methods for <see cref="_ITestCase"/>.
/// </summary>
public static class TestCaseExtensions
{
	/// <summary>
	/// Converts an instance of <see cref="_ITestCase"/> into <see cref="_TestCaseDiscovered"/> for reporting
	/// back to a remote meta-runner.
	/// </summary>
	public static _TestCaseDiscovered ToTestCaseDiscovered(this _ITestCase testCase)
	{
		Guard.ArgumentNotNull(testCase);

		return new _TestCaseDiscovered()
		{
			AssemblyUniqueID = testCase.TestCollection.TestAssembly.UniqueID,
			Serialization = SerializationHelper.Serialize(testCase),
			SkipReason = testCase.SkipReason,
			SourceFilePath = testCase.SourceFilePath,
			SourceLineNumber = testCase.SourceLineNumber,
			TestCaseDisplayName = testCase.TestCaseDisplayName,
			TestCaseUniqueID = testCase.UniqueID,
			TestClassName = testCase.TestClassName,
			TestClassNamespace = testCase.TestClassNamespace,
			TestClassNameWithNamespace = testCase.TestClassNameWithNamespace,
			TestClassUniqueID = testCase.TestClass?.UniqueID,
			TestCollectionUniqueID = testCase.TestCollection.UniqueID,
			TestMethodName = testCase.TestMethodName,
			TestMethodUniqueID = testCase.TestMethod?.UniqueID,
			Traits = testCase.Traits,
		};
	}
}
