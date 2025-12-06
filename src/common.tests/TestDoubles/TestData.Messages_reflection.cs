#if !XUNIT_AOT

using System.Linq;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

// This file manufactures instances of the test messages (using reflection)
partial class TestData
{
	public static ITestCaseDiscovered TestCaseDiscovered<TClass>(
		string testMethod,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string? testCaseDisplayName = null)
	{
		var type = typeof(TClass);
		var methodInfo = Guard.NotNull($"Could not find method '{testMethod}' in type '{type.FullName}'", type.GetMethod(testMethod));
		var factAttribute = methodInfo.GetMatchingCustomAttributes<IFactAttribute>().FirstOrDefault();
		var @explicit = factAttribute?.Explicit ?? false;
		var skipReason = factAttribute?.Skip;
		var traits = ExtensibilityPointFactory.GetMethodTraits(methodInfo, testClassTraits: null);

		var testClassUniqueID = UniqueIDGenerator.ForTestClass(DefaultTestCollectionUniqueID, type.FullName);
		var testMethodUniqueID = UniqueIDGenerator.ForTestMethod(testClassUniqueID, testMethod);
		var testCaseUniqueID = UniqueIDGenerator.ForTestCase(testMethodUniqueID, null, null);

		return TestCaseDiscovered(
			DefaultAssemblyUniqueID,
			@explicit,
			DefaultTestCaseSerialization,
			skipReason,
			sourceFilePath,
			sourceLineNumber,
			testCaseDisplayName ?? $"{type.FullName}.{testMethod}",
			testCaseUniqueID,
			type.MetadataToken,
			type.FullName,
			type.Namespace,
			type.Name,
			testClassUniqueID,
			DefaultTestCollectionUniqueID,
			methodInfo.GetArity(),
			methodInfo.MetadataToken,
			testMethod,
			methodInfo.GetParameters().Select(p => p.ParameterType.ToVSTestTypeName()).ToArray(),
			methodInfo.ReturnType.ToVSTestTypeName(),
			testMethodUniqueID,
			traits
		);
	}

	public static ITestCaseStarting TestCaseStarting<TClass>(
		string testMethod,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string? testCaseDisplayName = null)
	{
		var type = typeof(TClass);
		var methodInfo = Guard.NotNull($"Could not find method '{testMethod}' in type '{type.FullName}'", type.GetMethod(testMethod));
		var factAttribute = methodInfo.GetMatchingCustomAttributes<IFactAttribute>().FirstOrDefault();
		var @explicit = factAttribute?.Explicit ?? false;
		var skipReason = factAttribute?.Skip;
		var traits = ExtensibilityPointFactory.GetMethodTraits(methodInfo, testClassTraits: null);

		var testClassUniqueID = UniqueIDGenerator.ForTestClass(DefaultTestCollectionUniqueID, type.FullName);
		var testMethodUniqueID = UniqueIDGenerator.ForTestMethod(testClassUniqueID, testMethod);
		var testCaseUniqueID = UniqueIDGenerator.ForTestCase(testMethodUniqueID, null, null);

		return TestCaseStarting(
			DefaultAssemblyUniqueID,
			@explicit,
			skipReason,
			sourceFilePath,
			sourceLineNumber,
			testCaseDisplayName ?? $"{type.FullName}.{testMethod}",
			testCaseUniqueID,
			type.MetadataToken,
			type.FullName,
			type.Namespace,
			type.Name,
			testClassUniqueID,
			DefaultTestCollectionUniqueID,
			methodInfo.GetArity(),
			methodInfo.MetadataToken,
			testMethod,
			methodInfo.GetParameters().Select(p => p.ParameterType.ToVSTestTypeName()).ToArray(),
			methodInfo.ReturnType.ToVSTestTypeName(),
			testMethodUniqueID,
			traits
		);
	}
}

#endif  // !XUNIT_AOT
