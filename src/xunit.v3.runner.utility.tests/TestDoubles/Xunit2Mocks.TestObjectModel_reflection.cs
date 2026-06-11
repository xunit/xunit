#pragma warning disable xUnit3000 // These derive from the "wrong" version (v3 instead of v2) of LLMBRO, which is acceptable

using Xunit.Abstractions;
using LongLivedMarshalByRefObject = Xunit.Sdk.LongLivedMarshalByRefObject;

namespace Xunit.Runner.v2;

// This file manufactures mocks of the test object model interfaces.
partial class Xunit2Mocks
{
	public static ITest Test(
		string displayName,
		ITestCase testCase) =>
			new MockTest
			{
				DisplayName = displayName,
				TestCase = testCase,
			};

	class MockTest : LongLivedMarshalByRefObject, ITest
	{
		public required string DisplayName { get; set; }
		public required ITestCase TestCase { get; set; }
	}

	public static ITestAssembly TestAssembly(
		string assemblyFileName = "testAssembly.dll",
		string? configFileName = null,
		IReflectionAttributeInfo[]? customAttributes = null,
		string targetFrameworkName = ".MockEnvironment,Version=v21.12",
		ITypeInfo[]? types = null) =>
			new MockTestAssembly
			{
				Assembly = AssemblyInfo(assemblyFileName, [.. customAttributes ?? [], TargetFrameworkAttribute(targetFrameworkName)], types),
				ConfigFileName = configFileName,
			};

	class MockTestAssembly : LongLivedMarshalByRefObject, ITestAssembly
	{
		public required IAssemblyInfo Assembly { get; set; }
		public required string? ConfigFileName { get; set; }

		public void Deserialize(IXunitSerializationInfo info) => throw new InvalidOperationException("Using ITestAssembly.Deserialize while testing is prohibited");
		public void Serialize(IXunitSerializationInfo info) => throw new InvalidOperationException("Using ITestAssembly.Serialize while testing is prohibited");
	}

	public static ITestCase TestCase(
		ITestMethod testMethod,
		string displayName = "<unset>",
		string? skipReason = null,
		string? sourceFileName = null,
		int? sourceLineNumber = null,
		object?[]? testMethodArguments = null,
		Dictionary<string, List<string>>? traits = null,
		string uniqueID = "test-case-uniqueid")
	{
		var sourceInformation =
			sourceFileName is not null
				? new Xunit2SourceInformation { FileName = sourceFileName, LineNumber = sourceLineNumber }
				: null;

		return new MockTestCase
		{
			DisplayName = displayName,
			SkipReason = skipReason,
			SourceInformation = sourceInformation,
			TestMethod = testMethod,
			TestMethodArguments = testMethodArguments ?? [],
			Traits = traits ?? EmptyTraits,
			UniqueID = uniqueID,
		};
	}

	class MockTestCase : LongLivedMarshalByRefObject, ITestCase
	{
		public required string DisplayName { get; set; }
		public required string? SkipReason { get; set; }
		public required ISourceInformation? SourceInformation { get; set; }
		public required ITestMethod TestMethod { get; set; }
		public required object?[] TestMethodArguments { get; set; }
		public required Dictionary<string, List<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		public void Deserialize(IXunitSerializationInfo info) => throw new InvalidOperationException("Using ITestCase.Deserialize while testing is prohibited");
		public void Serialize(IXunitSerializationInfo info) => throw new InvalidOperationException("Using ITestCase.Serialize while testing is prohibited");
	}

	public static ITestClass TestClass(
		ITypeInfo? @class = null,
		ITestCollection? testCollection = null) =>
			new MockTestClass
			{
				Class = @class ?? TypeInfo(),
				TestCollection = testCollection ?? TestCollection(),
			};

	class MockTestClass : LongLivedMarshalByRefObject, ITestClass
	{
		public required ITypeInfo Class { get; set; }
		public required ITestCollection TestCollection { get; set; }

		public void Deserialize(IXunitSerializationInfo info) => throw new InvalidOperationException("Using ITestClass.Deserialize while testing is prohibited");
		public void Serialize(IXunitSerializationInfo info) => throw new InvalidOperationException("Using ITestClass.Serialize while testing is prohibited");
	}

	public static ITestCollection TestCollection(
		ITypeInfo? collectionDefinition = null,
		string? displayName = null,
		ITestAssembly? testAssembly = null,
		Guid? uniqueID = null) =>
			new MockTestCollection
			{
				CollectionDefinition = collectionDefinition,
				DisplayName = displayName ?? "Mock test collection",
				TestAssembly = testAssembly ?? TestAssembly(),
				UniqueID = uniqueID ?? OneGuid,
			};

	class MockTestCollection : LongLivedMarshalByRefObject, ITestCollection
	{
		public required ITypeInfo? CollectionDefinition { get; set; }
		public required string DisplayName { get; set; }
		public required ITestAssembly TestAssembly { get; set; }
		public required Guid UniqueID { get; set; }

		public void Deserialize(IXunitSerializationInfo info) => throw new InvalidOperationException("Using ITestCollection.Deserialize while testing is prohibited");
		public void Serialize(IXunitSerializationInfo info) => throw new InvalidOperationException("Using ITestCollection.Serialize while testing is prohibited");
	}

	public static ITestMethod TestMethod(
		string methodName,
		ITestClass testClass) =>
			new MockTestMethod
			{
				Method = MethodInfo(name: methodName, type: testClass.Class),
				TestClass = testClass,
			};

	class MockTestMethod : LongLivedMarshalByRefObject, ITestMethod
	{
		public required IMethodInfo Method { get; set; }
		public required ITestClass TestClass { get; set; }

		public void Deserialize(IXunitSerializationInfo info) => throw new InvalidOperationException("Using ITestMethod.Deserialize while testing is prohibited");
		public void Serialize(IXunitSerializationInfo info) => throw new InvalidOperationException("Using ITestMethod.Serialize while testing is prohibited");
	}
}
