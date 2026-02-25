using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

// This file manufactures mocks of the test object model interfaces.
partial class Mocks
{
	// ===== ITestXxx =====

	public static ITest Test(
		ITestCase? testCase = null,
		string testDisplayName = TestData.DefaultTestDisplayName,
		string? testLabel = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestUniqueID) =>
			new MockTest
			{
				TestCase = testCase ?? TestCase(),
				TestDisplayName = testDisplayName,
				TestLabel = testLabel,
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockTest : ITest
	{
		public required ITestCase TestCase { get; set; }
		public required string TestDisplayName { get; set; }
		public required string? TestLabel { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }
	}

	public static ITestAssembly TestAssembly(
		string assemblyName = TestData.DefaultAssemblyName,
		string assemblyPath = TestData.DefaultAssemblyPath,
		string? configFilePath = null,
		Guid? moduleVersionID = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultAssemblyUniqueID) =>
			new MockTestAssembly
			{
				AssemblyName = assemblyName,
				AssemblyPath = assemblyPath,
				ConfigFilePath = configFilePath,
				ModuleVersionID = moduleVersionID ?? TestData.DefaultModuleVersionID,
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockTestAssembly : ITestAssembly
	{
		public required string AssemblyName { get; set; }
		public required string AssemblyPath { get; set; }
		public required string? ConfigFilePath { get; set; }
		public required Guid ModuleVersionID { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }
	}

	public static ITestCase TestCase(
		bool @explicit = false,
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = TestData.DefaultTestCaseDisplayName,
		int testClassMetadataToken = TestData.DefaultTestClassMetadataToken,
		ITestMethod? testMethod = null,
		int testMethodMetadataToken = TestData.DefaultTestMethodMetadataToken,
		string[]? testMethodParameterTypesVSTest = null,
		string testMethodReturnTypeVSTest = "System.Void",
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestCaseUniqueID)
	{
		testMethod ??= TestMethod();

		return new MockTestCase
		{
			Explicit = @explicit,
			SkipReason = skipReason,
			SourceFilePath = sourceFilePath,
			SourceLineNumber = sourceLineNumber,
			TestCaseDisplayName = testCaseDisplayName,
			TestCollection = testMethod.TestClass.TestCollection,
			TestClass = testMethod.TestClass,
			TestClassMetadataToken = testClassMetadataToken,
			TestMethod = testMethod,
			TestMethodMetadataToken = testMethodMetadataToken,
			TestMethodParameterTypesVSTest = testMethodParameterTypesVSTest,
			TestMethodReturnTypeVSTest = testMethodReturnTypeVSTest,
			Traits = traits ?? TestData.DefaultTraits,
			UniqueID = uniqueID,
		};
	}

	public static ITestCase TestCase(
		ITestCollection testCollection,
		bool @explicit = false,
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = TestData.DefaultTestCaseDisplayName,
		ITestClass? testClass = null,
		int testClassMetadataToken = TestData.DefaultTestClassMetadataToken,
		ITestMethod? testMethod = null,
		int testMethodMetadataToken = TestData.DefaultTestMethodMetadataToken,
		string[]? testMethodParameterTypesVSTest = null,
		string testMethodReturnTypeVSTest = "System.Void",
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestCaseUniqueID) =>
			new MockTestCase
			{
				Explicit = @explicit,
				SkipReason = skipReason,
				SourceFilePath = sourceFilePath,
				SourceLineNumber = sourceLineNumber,
				TestCaseDisplayName = testCaseDisplayName,
				TestCollection = testCollection,
				TestClass = testClass,
				TestClassMetadataToken = testClassMetadataToken,
				TestMethod = testMethod,
				TestMethodMetadataToken = testMethodMetadataToken,
				TestMethodParameterTypesVSTest = testMethodParameterTypesVSTest,
				TestMethodReturnTypeVSTest = testMethodReturnTypeVSTest,
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockTestCase : ITestCase
	{
		public required bool Explicit { get; set; }
		public required string? SkipReason { get; set; }
		public required string? SourceFilePath { get; set; }
		public required int? SourceLineNumber { get; set; }
		public required ITestClass? TestClass { get; set; }
		public required string TestCaseDisplayName { get; set; }
		public required ITestCollection TestCollection { get; set; }
		public required int? TestClassMetadataToken { get; set; }
		public required ITestMethod? TestMethod { get; set; }
		public required int? TestMethodMetadataToken { get; set; }
		public required string[]? TestMethodParameterTypesVSTest { get; set; }
		public required string? TestMethodReturnTypeVSTest { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		public string? TestClassName => TestClass?.TestClassName;
		public string? TestClassNamespace => TestClass?.TestClassNamespace;
		public string? TestClassSimpleName => TestClass?.TestClassSimpleName;
		public int? TestMethodArity => TestMethod?.MethodArity;
		public string? TestMethodName => TestMethod?.MethodName;
	}

	public static ITestCase TestCaseAsyncDisposable(
		Action asyncDisposeCallback,
		bool @explicit = false,
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = TestData.DefaultTestCaseDisplayName,
		int testClassMetadataToken = TestData.DefaultTestClassMetadataToken,
		ITestMethod? testMethod = null,
		int testMethodMetadataToken = TestData.DefaultTestMethodMetadataToken,
		string[]? testMethodParameterTypesVSTest = null,
		string testMethodReturnTypeVSTest = "System.Void",
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestCaseUniqueID)
	{
		testMethod ??= TestMethod();

		return new MockTestCaseAsyncDisposable(asyncDisposeCallback)
		{
			Explicit = @explicit,
			SkipReason = skipReason,
			SourceFilePath = sourceFilePath,
			SourceLineNumber = sourceLineNumber,
			TestCaseDisplayName = testCaseDisplayName,
			TestCollection = testMethod.TestClass.TestCollection,
			TestClass = testMethod.TestClass,
			TestClassMetadataToken = testClassMetadataToken,
			TestMethod = testMethod,
			TestMethodMetadataToken = testMethodMetadataToken,
			TestMethodParameterTypesVSTest = testMethodParameterTypesVSTest,
			TestMethodReturnTypeVSTest = testMethodReturnTypeVSTest,
			Traits = traits ?? TestData.DefaultTraits,
			UniqueID = uniqueID,
		};
	}

	class MockTestCaseAsyncDisposable(Action asyncDisposeCallback) :
		MockTestCase, IAsyncDisposable
	{
		public ValueTask DisposeAsync()
		{
			asyncDisposeCallback();
			return default;
		}
	}

	public static ITestCase TestCaseDisposable(
		Action disposeCallback,
		bool @explicit = false,
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = TestData.DefaultTestCaseDisplayName,
		int testClassMetadataToken = TestData.DefaultTestClassMetadataToken,
		ITestMethod? testMethod = null,
		int testMethodMetadataToken = TestData.DefaultTestMethodMetadataToken,
		string[]? testMethodParameterTypesVSTest = null,
		string testMethodReturnTypeVSTest = "System.Void",
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestCaseUniqueID)
	{
		testMethod ??= TestMethod();

		return new MockTestCaseDisposable(disposeCallback)
		{
			Explicit = @explicit,
			SkipReason = skipReason,
			SourceFilePath = sourceFilePath,
			SourceLineNumber = sourceLineNumber,
			TestCaseDisplayName = testCaseDisplayName,
			TestCollection = testMethod.TestClass.TestCollection,
			TestClass = testMethod.TestClass,
			TestClassMetadataToken = testClassMetadataToken,
			TestMethod = testMethod,
			TestMethodMetadataToken = testMethodMetadataToken,
			TestMethodParameterTypesVSTest = testMethodParameterTypesVSTest,
			TestMethodReturnTypeVSTest = testMethodReturnTypeVSTest,
			Traits = traits ?? TestData.DefaultTraits,
			UniqueID = uniqueID,
		};
	}

	class MockTestCaseDisposable(Action disposeCallback) :
		MockTestCase, IDisposable
	{
		public void Dispose() => disposeCallback();
	}

	public static ITestClass TestClass(
		string testClassName = TestData.DefaultTestClassName,
		string? testClassNamespace = null,
		string testClassSimpleName = TestData.DefaultTestClassSimpleName,
		ITestCollection? testCollection = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestClassUniqueID) =>
			new MockTestClass
			{
				TestClassName = testClassName,
				TestClassNamespace = testClassNamespace,
				TestClassSimpleName = testClassSimpleName,
				TestCollection = testCollection ?? TestCollection(),
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockTestClass : ITestClass
	{
		public required string TestClassName { get; set; }
		public required string? TestClassNamespace { get; set; }
		public required string TestClassSimpleName { get; set; }
		public required ITestCollection TestCollection { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }
	}

	public static ITestCollection TestCollection(
		ITestAssembly? testAssembly = null,
		string? testCollectionClassName = null,
		string testCollectionDisplayName = TestData.DefaultTestCollectionDisplayName,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestCollectionUniqueID) =>
			new MockTestCollection
			{
				TestAssembly = testAssembly ?? TestAssembly(),
				TestCollectionClassName = testCollectionClassName,
				TestCollectionDisplayName = testCollectionDisplayName,
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockTestCollection : ITestCollection
	{
		public required ITestAssembly TestAssembly { get; set; }
		public required string? TestCollectionClassName { get; set; }
		public required string TestCollectionDisplayName { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }
	}

	public static ITestMethod TestMethod(
		int methodArity = 0,
		string methodName = TestData.DefaultMethodName,
		ITestClass? testClass = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestMethodUniqueID) =>
			new MockTestMethod
			{
				MethodArity = methodArity,
				MethodName = methodName,
				TestClass = testClass ?? TestClass(),
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockTestMethod : ITestMethod
	{
		public required int? MethodArity { get; set; }
		public required string MethodName { get; set; }
		public required ITestClass TestClass { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }
	}

	// ===== ICoreTestXxx =====

	public static ICoreTest CoreTest(
		bool @explicit = false,
		ICoreTestCase? testCase = null,
		string testDisplayName = TestData.DefaultTestDisplayName,
		string? testLabel = null,
		int timeout = 0,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestUniqueID) =>
			new MockCoreTest
			{
				Explicit = @explicit,
				TestCase = testCase ?? CoreTestCase(),
				TestDisplayName = testDisplayName,
				TestLabel = testLabel,
				Timeout = timeout,
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockCoreTest : ICoreTest
	{
		public required bool Explicit { get; set; }
		public required ICoreTestCase TestCase { get; set; }
		public required string TestDisplayName { get; set; }
		public required string? TestLabel { get; set; }
		public required int Timeout { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		ITestCase ITest.TestCase => TestCase;
	}

	public static ICoreTestAssembly CoreTestAssembly(
		string assemblyName = TestData.DefaultAssemblyName,
		string assemblyPath = TestData.DefaultAssemblyPath,
		string? configFilePath = null,
		bool? disableParallelization = null,
		int? maxParallelThreads = null,
		Guid? moduleVersionID = null,
		ParallelAlgorithm? parallelAlgorithm = null,
		string targetFramework = TestData.DefaultTargetFramework,
		ITestCaseOrderer? testCaseOrderer = null,
		ITestClassOrderer? testClassOrderer = null,
		ITestCollectionOrderer? testCollectionOrderer = null,
		ITestMethodOrderer? testMethodOrderer = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultAssemblyUniqueID,
		Version? version = null) =>
			new MockCoreTestAssembly
			{
				AssemblyName = assemblyName,
				AssemblyPath = assemblyPath,
				ConfigFilePath = configFilePath,
				DisableParallelization = disableParallelization,
				MaxParallelThreads = maxParallelThreads,
				ModuleVersionID = moduleVersionID ?? TestData.DefaultModuleVersionID,
				ParallelAlgorithm = parallelAlgorithm,
				TargetFramework = targetFramework,
				TestCaseOrderer = testCaseOrderer,
				TestClassOrderer = testClassOrderer,
				TestCollectionOrderer = testCollectionOrderer,
				TestMethodOrderer = testMethodOrderer,
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
				Version = version ?? new Version(2112, 42, 2600),
			};

	class MockCoreTestAssembly : ICoreTestAssembly
	{
		public required string AssemblyName { get; set; }
		public required string AssemblyPath { get; set; }
		public required string? ConfigFilePath { get; set; }
		public required bool? DisableParallelization { get; set; }
		public required int? MaxParallelThreads { get; set; }
		public required Guid ModuleVersionID { get; set; }
		public required ParallelAlgorithm? ParallelAlgorithm { get; set; }
		public required string TargetFramework { get; set; }
		public required ITestCaseOrderer? TestCaseOrderer { get; set; }
		public required ITestClassOrderer? TestClassOrderer { get; set; }
		public required ITestCollectionOrderer? TestCollectionOrderer { get; set; }
		public required ITestMethodOrderer? TestMethodOrderer { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }
		public required Version Version { get; set; }

		public Assembly Assembly => throw new NotSupportedException("Using ICoreTestAssembly.Assembly while testing is prohibited");
	}

	public static ICoreTestCase CoreTestCase(
		bool @explicit = false,
		Action? postInvoke = null,
		Action? preInvoke = null,
		Type[]? skipExceptions = null,
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = TestData.DefaultTestCaseDisplayName,
		int testClassMetadataToken = TestData.DefaultTestClassMetadataToken,
		ICoreTestMethod? testMethod = null,
		int testMethodMetadataToken = TestData.DefaultTestMethodMetadataToken,
		string[]? testMethodParameterTypesVSTest = null,
		string testMethodReturnTypeVSTest = "System.Void",
		int timeout = 0,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestCaseUniqueID) =>
			new MockCoreTestCase(preInvoke, postInvoke)
			{
				Explicit = @explicit,
				SkipExceptions = skipExceptions,
				SkipReason = skipReason,
				SourceFilePath = sourceFilePath,
				SourceLineNumber = sourceLineNumber,
				TestCaseDisplayName = testCaseDisplayName,
				TestClassMetadataToken = testClassMetadataToken,
				TestMethod = testMethod ?? CoreTestMethod(),
				TestMethodMetadataToken = testMethodMetadataToken,
				TestMethodParameterTypesVSTest = testMethodParameterTypesVSTest,
				TestMethodReturnTypeVSTest = testMethodReturnTypeVSTest,
				Timeout = timeout,
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockCoreTestCase(
		Action? preInvoke,
		Action? postInvoke) :
			ICoreTestCase
	{
		public required bool Explicit { get; set; }
		public required Type[]? SkipExceptions { get; set; }
		public required string? SkipReason { get; set; }
		public required string? SourceFilePath { get; set; }
		public required int? SourceLineNumber { get; set; }
		public required string TestCaseDisplayName { get; set; }
		public required int? TestClassMetadataToken { get; set; }
		public required ICoreTestMethod TestMethod { get; set; }
		public required int? TestMethodMetadataToken { get; set; }
		public required string[]? TestMethodParameterTypesVSTest { get; set; }
		public required string? TestMethodReturnTypeVSTest { get; set; }
		public required int Timeout { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		public ICoreTestClass TestClass => TestMethod.TestClass;
		ITestClass? ITestCase.TestClass => TestClass;
		public string TestClassName => TestClass.TestClassName;
		public string? TestClassNamespace => TestClass.TestClassNamespace;
		public string TestClassSimpleName => TestClass.TestClassSimpleName;
		public ICoreTestCollection TestCollection => TestClass.TestCollection;
		ITestCollection ITestCase.TestCollection => TestCollection;
		ITestMethod? ITestCase.TestMethod => TestMethod;
		public int TestMethodArity => TestMethod.MethodArity;
		int? ITestCaseMetadata.TestMethodArity => TestMethodArity;
		public string TestMethodName => TestMethod.MethodName;

		public void PostInvoke() => postInvoke?.Invoke();
		public void PreInvoke() => preInvoke?.Invoke();
	}

	public static ICoreTestClass CoreTestClass(
		ITestCaseOrderer? testCaseOrderer = null,
		string testClassName = TestData.DefaultTestClassName,
		string testClassNamespace = TestData.DefaultTestClassNamespace,
		string testClassSimpleName = TestData.DefaultTestClassSimpleName,
		ICoreTestCollection? testCollection = null,
		ITestMethodOrderer? testMethodOrderer = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestClassUniqueID) =>
			new MockCoreTestClass
			{
				TestCaseOrderer = testCaseOrderer,
				TestClassName = testClassName,
				TestClassNamespace = testClassNamespace,
				TestClassSimpleName = testClassSimpleName,
				TestCollection = testCollection ?? CoreTestCollection(),
				TestMethodOrderer = testMethodOrderer,
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockCoreTestClass : ICoreTestClass
	{
		public required ITestCaseOrderer? TestCaseOrderer { get; set; }
		public required string TestClassName { get; set; }
		public required string? TestClassNamespace { get; set; }
		public required string TestClassSimpleName { get; set; }
		public required ICoreTestCollection TestCollection { get; set; }
		public required ITestMethodOrderer? TestMethodOrderer { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		public Type Class => throw new NotSupportedException("Using ICoreTestClass.Class while testing is prohibited");
		ITestCollection ITestClass.TestCollection => TestCollection;
	}

	public static ICoreTestCollection CoreTestCollection(
		bool disableParallelization = false,
		ICoreTestAssembly? testAssembly = null,
		ITestCaseOrderer? testCaseOrderer = null,
		ITestClassOrderer? testClassOrderer = null,
		string? testCollectionClassName = null,
		string testCollectionDisplayName = TestData.DefaultTestCollectionDisplayName,
		ITestMethodOrderer? testMethodOrderer = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestCollectionUniqueID) =>
			new MockCoreTestCollection
			{
				DisableParallelization = disableParallelization,
				TestAssembly = testAssembly ?? CoreTestAssembly(),
				TestCaseOrderer = testCaseOrderer,
				TestClassOrderer = testClassOrderer,
				TestCollectionClassName = testCollectionClassName,
				TestCollectionDisplayName = testCollectionDisplayName,
				TestMethodOrderer = testMethodOrderer,
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockCoreTestCollection : ICoreTestCollection
	{
		public required bool DisableParallelization { get; set; }
		public required ICoreTestAssembly TestAssembly { get; set; }
		public required ITestCaseOrderer? TestCaseOrderer { get; set; }
		public required ITestClassOrderer? TestClassOrderer { get; set; }
		public required string? TestCollectionClassName { get; set; }
		public required string TestCollectionDisplayName { get; set; }
		public required ITestMethodOrderer? TestMethodOrderer { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		ITestAssembly ITestCollection.TestAssembly => TestAssembly;
	}

	public static ICoreTestMethod CoreTestMethod(
		int methodArity = 0,
		string methodName = TestData.DefaultMethodName,
		ITestCaseOrderer? testCaseOrderer = null,
		ICoreTestClass? testClass = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestMethodUniqueID) =>
			new MockCoreTestMethod
			{
				MethodArity = methodArity,
				MethodName = methodName,
				TestCaseOrderer = testCaseOrderer,
				TestClass = testClass ?? CoreTestClass(),
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockCoreTestMethod : ICoreTestMethod
	{
		public required int MethodArity { get; set; }
		public required string MethodName { get; set; }
		public required ITestCaseOrderer? TestCaseOrderer { get; set; }
		public required ICoreTestClass TestClass { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		ITestClass ITestMethod.TestClass => TestClass;
		int? ITestMethodMetadata.MethodArity => MethodArity;
	}
}
