using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// This file manufactures mocks of the test object model interfaces. The generic version based on a
// real test class will use live objects from TestData for the parents.
public static partial class Mocks
{
	// ===== IXunitTestXxx =====

	public static IXunitTest XunitTest(
		bool @explicit = false,
		string? skipReason = null,
		Type? skipType = null,
		string? skipUnless = null,
		string? skipWhen = null,
		IXunitTestCase? testCase = null,
		string testDisplayName = TestData.DefaultTestDisplayName,
		string? testLabel = null,
		object?[]? testMethodArguments = null,
		int timeout = 0,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestUniqueID)
	{
		testCase ??= XunitTestCase();

		return new MockXunitTest
		{
			Explicit = @explicit,
			SkipReason = skipReason,
			SkipType = skipType,
			SkipUnless = skipUnless,
			SkipWhen = skipWhen,
			TestCase = testCase,
			TestDisplayName = testDisplayName,
			TestLabel = testLabel,
			TestMethodArguments = testMethodArguments ?? [],
			Timeout = timeout,
			Traits = traits ?? testCase.Traits,
			UniqueID = uniqueID,
		};
	}

	class MockXunitTest : IXunitTest
	{
		public required bool Explicit { get; set; }
		public required string? SkipReason { get; set; }
		public required Type? SkipType { get; set; }
		public required string? SkipUnless { get; set; }
		public required string? SkipWhen { get; set; }
		public required IXunitTestCase TestCase { get; set; }
		public required string TestDisplayName { get; set; }
		public required string? TestLabel { get; set; }
		public required object?[] TestMethodArguments { get; set; }
		public required int Timeout { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		public IXunitTestMethod TestMethod => TestCase.TestMethod;

		ICoreTestCase ICoreTest.TestCase => TestCase;
		ITestCase ITest.TestCase => TestCase;
	}

	public static IXunitTestAssembly XunitTestAssembly(
		IReadOnlyCollection<Type>? assemblyFixtureTypes = null,
		string assemblyName = TestData.DefaultAssemblyName,
		string assemblyPath = TestData.DefaultAssemblyPath,
		IReadOnlyCollection<IBeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		ICollectionBehaviorAttribute? collectionBehavior = null,
		IReadOnlyDictionary<string, (Type Type, CollectionDefinitionAttribute Attribute)>? collectionDefinitions = null,
		string? configFilePath = null,
		Guid? moduleVersionID = null,
		string targetFramework = TestData.DefaultTargetFramework,
		ITestCaseOrderer? testCaseOrderer = null,
		ITestClassOrderer? testClassOrderer = null,
		ITestCollectionOrderer? testCollectionOrderer = null,
		ITestMethodOrderer? testMethodOrderer = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultAssemblyUniqueID,
		Version? version = null) =>
			new MockXunitTestAssembly
			{
				AssemblyFixtureTypes = assemblyFixtureTypes ?? [],
				AssemblyName = assemblyName,
				AssemblyPath = assemblyPath,
				BeforeAfterTestAttributes = beforeAfterTestAttributes ?? [],
				CollectionBehavior = collectionBehavior,
				CollectionDefinitions = collectionDefinitions ?? TestData.EmptyCollectionDefinitions,
				ConfigFilePath = configFilePath,
				ModuleVersionID = moduleVersionID ?? TestData.DefaultModuleVersionID,
				TargetFramework = targetFramework,
				TestCaseOrderer = testCaseOrderer,
				TestClassOrderer = testClassOrderer,
				TestCollectionOrderer = testCollectionOrderer,
				TestMethodOrderer = testMethodOrderer,
				Traits = traits ?? TestData.EmptyTraits,
				UniqueID = uniqueID,
				Version = version ?? TestData.DefaultVersion,
			};

	class MockXunitTestAssembly : IXunitTestAssembly
	{
		public required IReadOnlyCollection<Type> AssemblyFixtureTypes { get; set; }
		public required string AssemblyName { get; set; }
		public required string AssemblyPath { get; set; }
		public required IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes { get; set; }
		public required ICollectionBehaviorAttribute? CollectionBehavior { get; set; }
		public required IReadOnlyDictionary<string, (Type Type, CollectionDefinitionAttribute Attribute)> CollectionDefinitions { get; set; }
		public required string? ConfigFilePath { get; set; }
		public required Guid ModuleVersionID { get; set; }
		public required string TargetFramework { get; set; }
		public required ITestCaseOrderer? TestCaseOrderer { get; set; }
		public required ITestClassOrderer? TestClassOrderer { get; set; }
		public required ITestCollectionOrderer? TestCollectionOrderer { get; set; }
		public required ITestMethodOrderer? TestMethodOrderer { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }
		public required Version Version { get; set; }

		public Assembly Assembly => throw new InvalidOperationException("Using IXunitTestAssembly.Assembly while testing is prohibited");
		public bool? DisableParallelization => CollectionBehavior?.DisableTestParallelization;
		public int? MaxParallelThreads => CollectionBehavior?.MaxParallelThreads;
		public ParallelAlgorithm? ParallelAlgorithm => CollectionBehavior?.ParallelAlgorithm;
	}

	public static IXunitTestCase XunitTestCase(
		Action? asyncDisposeCallback = null,
		Action? disposeCallback = null,
		bool @explicit = false,
		Type[]? skipExceptions = null,
		string? skipReason = null,
		Type? skipType = null,
		string? skipUnless = null,
		string? skipWhen = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = TestData.DefaultTestCaseDisplayName,
		int testClassMetadataToken = TestData.DefaultTestClassMetadataToken,
		IXunitTestMethod? testMethod = null,
		int testMethodMetadataToken = TestData.DefaultTestMethodMetadataToken,
		string[]? testMethodParameterTypesVSTest = null,
		string testMethodReturnTypeVSTest = "System.Void",
		int timeout = 0,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestCaseUniqueID)
	{
		testMethod ??= XunitTestMethod();
		traits ??= testMethod.Traits;

		if (asyncDisposeCallback is not null)
		{
			if (disposeCallback is not null)
				throw new ArgumentException("Can only define one disposal callback", nameof(disposeCallback));

			return new MockXunitTestCaseAsyncDisposable(asyncDisposeCallback)
			{
				Explicit = @explicit,
				SkipExceptions = skipExceptions,
				SkipReason = skipReason,
				SkipType = skipType,
				SkipUnless = skipUnless,
				SkipWhen = skipWhen,
				SourceFilePath = sourceFilePath,
				SourceLineNumber = sourceLineNumber,
				TestCaseDisplayName = testCaseDisplayName,
				TestClassMetadataToken = testClassMetadataToken,
				TestMethod = testMethod,
				TestMethodMetadataToken = testMethodMetadataToken,
				TestMethodParameterTypesVSTest = testMethodParameterTypesVSTest ?? [],
				TestMethodReturnTypeVSTest = testMethodReturnTypeVSTest,
				Timeout = timeout,
				Traits = traits,
				UniqueID = uniqueID,
			};
		}
		else if (disposeCallback is not null)
			return new MockXunitTestCaseDisposable(disposeCallback)
			{
				Explicit = @explicit,
				SkipExceptions = skipExceptions,
				SkipReason = skipReason,
				SkipType = skipType,
				SkipUnless = skipUnless,
				SkipWhen = skipWhen,
				SourceFilePath = sourceFilePath,
				SourceLineNumber = sourceLineNumber,
				TestCaseDisplayName = testCaseDisplayName,
				TestClassMetadataToken = testClassMetadataToken,
				TestMethod = testMethod,
				TestMethodMetadataToken = testMethodMetadataToken,
				TestMethodParameterTypesVSTest = testMethodParameterTypesVSTest ?? [],
				TestMethodReturnTypeVSTest = testMethodReturnTypeVSTest,
				Timeout = timeout,
				Traits = traits,
				UniqueID = uniqueID,
			};
		else
			return new MockXunitTestCase
			{
				Explicit = @explicit,
				SkipExceptions = skipExceptions,
				SkipReason = skipReason,
				SkipType = skipType,
				SkipUnless = skipUnless,
				SkipWhen = skipWhen,
				SourceFilePath = sourceFilePath,
				SourceLineNumber = sourceLineNumber,
				TestCaseDisplayName = testCaseDisplayName,
				TestClassMetadataToken = testClassMetadataToken,
				TestMethod = testMethod,
				TestMethodMetadataToken = testMethodMetadataToken,
				TestMethodParameterTypesVSTest = testMethodParameterTypesVSTest ?? [],
				TestMethodReturnTypeVSTest = testMethodReturnTypeVSTest,
				Timeout = timeout,
				Traits = traits,
				UniqueID = uniqueID,
			};
	}

#pragma warning disable xUnit3006 // Mock test cases do not need to implement serialization
#pragma warning disable xUnit3007 // Mock test cases do not need to implement serialization

	class MockXunitTestCase : IXunitTestCase
	{
		public required bool Explicit { get; set; }
		public required Type[]? SkipExceptions { get; set; }
		public required string? SkipReason { get; set; }
		public required Type? SkipType { get; set; }
		public required string? SkipUnless { get; set; }
		public required string? SkipWhen { get; set; }
		public required string? SourceFilePath { get; set; }
		public required int? SourceLineNumber { get; set; }
		public required string TestCaseDisplayName { get; set; }
		public required int? TestClassMetadataToken { get; set; }
		public required IXunitTestMethod TestMethod { get; set; }
		public required int TestMethodMetadataToken { get; set; }
		public required string[] TestMethodParameterTypesVSTest { get; set; }
		public required string TestMethodReturnTypeVSTest { get; set; }
		public required int Timeout { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		public IXunitTestClass TestClass => TestMethod.TestClass;
		public string TestClassName => TestClass.TestClassName;
		public string? TestClassNamespace => TestClass.TestClassNamespace;
		public string TestClassSimpleName => TestClass.TestClassSimpleName;
		public IXunitTestCollection TestCollection => TestClass.TestCollection;
		public int TestMethodArity => TestMethod.MethodArity;
		public string TestMethodName => TestMethod.MethodName;

		ICoreTestClass ICoreTestCase.TestClass => TestClass;
		ITestClass? ITestCase.TestClass => TestClass;
		ICoreTestCollection ICoreTestCase.TestCollection => TestCollection;
		ITestCollection ITestCase.TestCollection => TestCollection;
		ICoreTestMethod ICoreTestCase.TestMethod => TestMethod;
		ITestMethod? ITestCase.TestMethod => TestMethod;
		int? ITestCaseMetadata.TestMethodArity => TestMethodArity;
		int? ITestCaseMetadata.TestMethodMetadataToken => TestMethodMetadataToken;

		public ValueTask<IReadOnlyCollection<IXunitTest>> CreateTests() => new([]);
		public void PostInvoke() => throw new NotImplementedException();
		public void PreInvoke() => throw new NotImplementedException();
	}

	sealed class MockXunitTestCaseAsyncDisposable(Action callback) : MockXunitTestCase
	{
		public async ValueTask DisposeAsync() => callback();
	}

	sealed class MockXunitTestCaseDisposable(Action callback) : MockXunitTestCase
	{
		public void Dispose() => callback();
	}

#pragma warning restore xUnit3007
#pragma warning restore xUnit3006

	public static IXunitTestClass XunitTestClass(
		IReadOnlyCollection<IBeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		IReadOnlyCollection<Type>? classFixtureTypes = null,
		IReadOnlyCollection<ConstructorInfo>? constructors = null,
		int metadataToken = TestData.DefaultTestClassMetadataToken,
		IReadOnlyCollection<MethodInfo>? methods = null,
		ITestCaseOrderer? testCaseOrderer = null,
		string testClassName = TestData.DefaultTestClassName,
		string testClassNamespace = TestData.DefaultTestClassNamespace,
		string testClassSimpleName = TestData.DefaultTestClassSimpleName,
		IXunitTestCollection? testCollection = null,
		ITestMethodOrderer? testMethodOrderer = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestClassUniqueID)
	{
		testCollection ??= XunitTestCollection();

		return new MockXunitTestClass
		{
			BeforeAfterTestAttributes = beforeAfterTestAttributes ?? [],
			ClassFixtureTypes = classFixtureTypes ?? [],
			Constructors = constructors,
			MetadataToken = metadataToken,
			Methods = methods ?? [],
			TestCaseOrderer = testCaseOrderer,
			TestClassName = testClassName,
			TestClassNamespace = testClassNamespace,
			TestClassSimpleName = testClassSimpleName,
			TestCollection = testCollection,
			TestMethodOrderer = testMethodOrderer,
			Traits = traits ?? testCollection.Traits,
			UniqueID = uniqueID,
		};
	}

	class MockXunitTestClass : IXunitTestClass
	{
		public required IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes { get; set; }
		public required IReadOnlyCollection<Type> ClassFixtureTypes { get; set; }
		public required IReadOnlyCollection<ConstructorInfo>? Constructors { get; set; }
		public required int MetadataToken { get; set; }
		public required IReadOnlyCollection<MethodInfo> Methods { get; set; }
		public required ITestCaseOrderer? TestCaseOrderer { get; set; }
		public required string TestClassName { get; set; }
		public required string? TestClassNamespace { get; set; }
		public required string TestClassSimpleName { get; set; }
		public required IXunitTestCollection TestCollection { get; set; }
		public required ITestMethodOrderer? TestMethodOrderer { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		public Type Class => throw new InvalidOperationException("Using IXunitTestClass.Class while testing is prohibited");

		ICoreTestCollection ICoreTestClass.TestCollection => TestCollection;
		ITestCollection ITestClass.TestCollection => TestCollection;
	}

	public static IXunitTestCollection XunitTestCollection(
		IReadOnlyCollection<IBeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		IReadOnlyCollection<Type>? classFixtureTypes = null,
		IReadOnlyCollection<Type>? collectionFixtureTypes = null,
		bool disableParallelization = false,
		IXunitTestAssembly? testAssembly = null,
		ITestCaseOrderer? testCaseOrderer = null,
		ITestClassOrderer? testClassOrderer = null,
		ITestMethodOrderer? testMethodOrderer = null,
		string? testCollectionClassName = null,
		string testCollectionDisplayName = TestData.DefaultTestCollectionDisplayName,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestCollectionUniqueID)
	{
		testAssembly ??= XunitTestAssembly();

		return new MockXunitTestCollection
		{
			BeforeAfterTestAttributes = beforeAfterTestAttributes ?? [],
			ClassFixtureTypes = classFixtureTypes ?? [],
			CollectionFixtureTypes = collectionFixtureTypes ?? [],
			DisableParallelization = disableParallelization,
			TestAssembly = testAssembly,
			TestCaseOrderer = testCaseOrderer,
			TestClassOrderer = testClassOrderer,
			TestCollectionClassName = testCollectionClassName,
			TestCollectionDisplayName = testCollectionDisplayName,
			TestMethodOrderer = testMethodOrderer,
			Traits = traits ?? testAssembly.Traits,
			UniqueID = uniqueID,
		};
	}

	class MockXunitTestCollection : IXunitTestCollection
	{
		public required IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes { get; set; }
		public required IReadOnlyCollection<Type> ClassFixtureTypes { get; set; }
		public required bool DisableParallelization { get; set; }
		public required IReadOnlyCollection<Type> CollectionFixtureTypes { get; set; }
		public required IXunitTestAssembly TestAssembly { get; set; }
		public required ITestCaseOrderer? TestCaseOrderer { get; set; }
		public required ITestClassOrderer? TestClassOrderer { get; set; }
		public required string? TestCollectionClassName { get; set; }
		public required string TestCollectionDisplayName { get; set; }
		public required ITestMethodOrderer? TestMethodOrderer { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		public Type? CollectionDefinition => throw new InvalidOperationException("Using IXunitTestCollection.CollectionDefinition while testing is prohibited");

		ICoreTestAssembly ICoreTestCollection.TestAssembly => TestAssembly;
		ITestAssembly ITestCollection.TestAssembly => TestAssembly;
	}

	public static IXunitTestMethod XunitTestMethod(
		IReadOnlyCollection<IBeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		IReadOnlyCollection<IDataAttribute>? dataAttributes = null,
		string? displayName = null,
		IReadOnlyCollection<IFactAttribute>? factAttributes = null,
		bool isGenericMethodDefinition = false,
		int metadataToken = TestData.DefaultTestMethodMetadataToken,
		int methodArity = TestData.DefaultMethodArity,
		string methodName = TestData.DefaultMethodName,
		IReadOnlyCollection<ParameterInfo>? parameters = null,
		Type? returnType = null,
		ITestCaseOrderer? testCaseOrderer = null,
		IXunitTestClass? testClass = null,
		object?[]? testMethodArguments = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestMethodUniqueID)
	{
		testClass ??= XunitTestClass();

		return new MockXunitTestMethod(displayName)
		{
			BeforeAfterTestAttributes = beforeAfterTestAttributes ?? [],
			DataAttributes = dataAttributes ?? [],
			FactAttributes = factAttributes ?? [FactAttribute()],
			IsGenericMethodDefinition = isGenericMethodDefinition,
			MetadataToken = metadataToken,
			MethodArity = methodArity,
			MethodName = methodName,
			Parameters = parameters ?? [],
			ReturnType = returnType ?? typeof(void),
			TestCaseOrderer = testCaseOrderer,
			TestClass = testClass,
			TestMethodArguments = testMethodArguments ?? [],
			Traits = traits ?? testClass.Traits,
			UniqueID = uniqueID,
		};
	}

	class MockXunitTestMethod(string? displayName) : IXunitTestMethod
	{
		public required IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes { get; set; }
		public required IReadOnlyCollection<IDataAttribute> DataAttributes { get; set; }
		public required IReadOnlyCollection<IFactAttribute> FactAttributes { get; set; }
		public required bool IsGenericMethodDefinition { get; set; }
		public required int MetadataToken { get; set; }
		public required int MethodArity { get; set; }
		public required string MethodName { get; set; }
		public required IReadOnlyCollection<ParameterInfo> Parameters { get; set; }
		public required Type ReturnType { get; set; }
		public required ITestCaseOrderer? TestCaseOrderer { get; set; }
		public required IXunitTestClass TestClass { get; set; }
		public required object?[] TestMethodArguments { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		public MethodInfo Method => throw new InvalidOperationException("Using IXunitTestMethod.Method while testing is prohibited");

		ICoreTestClass ICoreTestMethod.TestClass => TestClass;
		ITestClass ITestMethod.TestClass => TestClass;
		int? ITestMethodMetadata.MethodArity => MethodArity;

		public string GetDisplayName(string baseDisplayName, string? label, object?[]? testMethodArguments, Type[]? methodGenericTypes) => displayName ?? baseDisplayName;
		public MethodInfo MakeGenericMethod(Type[] genericTypes) => throw new InvalidOperationException("Using IXunitTestMethod.MakeGenericMethod while testing is prohibited");
		public Type[]? ResolveGenericTypes(object?[] arguments) => throw new InvalidOperationException("Using IXunitTestMethod.ResolveGenericTypes while testing is prohibited");
		public object?[] ResolveMethodArguments(object?[] arguments) => throw new InvalidOperationException("Using IXunitTestMethod.ResolveMethodArguments while testing is prohibited");
	}
}
