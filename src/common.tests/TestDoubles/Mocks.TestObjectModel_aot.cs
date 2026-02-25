using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

// This file manufactures mocks of the test object model interfaces.
partial class Mocks
{
	// ===== ICodeGenTestXxx =====

	public static ICodeGenTest CodeGenTest(
		bool @explicit = false,
		Func<object?, ValueTask>? methodInvoker = null,
		string? skipReason = null,
		Func<bool>? skipUnless = null,
		Func<bool>? skipWhen = null,
		ICodeGenTestCase? testCase = null,
		string testDisplayName = TestData.DefaultTestDisplayName,
		string? testLabel = null,
		int timeout = 0,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestUniqueID) =>
			new MockCodeGenTest
			{
				Explicit = @explicit,
				MethodInvoker = methodInvoker ?? (_ => default),
				SkipReason = skipReason,
				SkipUnless = skipUnless,
				SkipWhen = skipWhen,
				TestCase = testCase ?? CodeGenTestCase(),
				TestDisplayName = testDisplayName,
				TestLabel = testLabel,
				Timeout = timeout,
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockCodeGenTest : ICodeGenTest
	{
		public required bool Explicit { get; set; }
		public required Func<object?, ValueTask> MethodInvoker { get; set; }
		public required string? SkipReason { get; set; }
		public required Func<bool>? SkipUnless { get; set; }
		public required Func<bool>? SkipWhen { get; set; }
		public required ICodeGenTestCase TestCase { get; set; }
		public required string TestDisplayName { get; set; }
		public required string? TestLabel { get; set; }
		public required int Timeout { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		ITestCase ITest.TestCase => TestCase;
		ICoreTestCase ICoreTest.TestCase => TestCase;
	}

	public static ICodeGenTestAssembly CodeGenTestAssembly(
		Assembly? assembly = null,
		IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>? assemblyFixtureFactories = null,
		string assemblyName = TestData.DefaultAssemblyName,
		string assemblyPath = TestData.DefaultAssemblyPath,
		IReadOnlyCollection<BeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		string? configFilePath = null,
		IReadOnlyDictionary<string, CodeGenTestCollectionRegistration>? collectionDefinitions = null,
		bool? disableParallelization = null,
		int? maxParallelThreads = null,
		Guid? moduleVersionID = null,
		ParallelAlgorithm? parallelAlgorithm = null,
		string targetFramework = TestData.DefaultTargetFramework,
		ITestCaseOrderer? testCaseOrderer = null,
		ITestClassOrderer? testClassOrderer = null,
		ICodeGenTestCollectionFactory? testCollectionFactory = null,
		ITestCollectionOrderer? testCollectionOrderer = null,
		ITestMethodOrderer? testMethodOrderer = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultAssemblyUniqueID,
		Version? version = null) =>
			new MockCodeGenTestAssembly(testCollectionFactory)
			{
				Assembly = assembly ?? typeof(Mocks).Assembly,
				AssemblyFixtureFactories = assemblyFixtureFactories ?? TestData.EmptyFixtureFactories,
				AssemblyName = assemblyName,
				AssemblyPath = assemblyPath,
				BeforeAfterTestAttributes = beforeAfterTestAttributes ?? [],
				CollectionDefinitions = collectionDefinitions ?? TestData.EmptyCollectionDefinitions,
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

	class MockCodeGenTestAssembly : ICodeGenTestAssembly
	{
		public MockCodeGenTestAssembly(ICodeGenTestCollectionFactory? testCollectionFactory)
		{
			TestCollectionFactory = testCollectionFactory ?? new CollectionPerClassTestCollectionFactory(this);
		}

		public required Assembly Assembly { get; set; }
		public required IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> AssemblyFixtureFactories { get; set; }
		public required string AssemblyName { get; set; }
		public required string AssemblyPath { get; set; }
		public required IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; set; }
		public required string? ConfigFilePath { get; set; }
		public required IReadOnlyDictionary<string, CodeGenTestCollectionRegistration> CollectionDefinitions { get; set; }
		public required bool? DisableParallelization { get; set; }
		public required int? MaxParallelThreads { get; set; }
		public required Guid ModuleVersionID { get; set; }
		public required ParallelAlgorithm? ParallelAlgorithm { get; set; }
		public required string TargetFramework { get; set; }
		public required ITestCaseOrderer? TestCaseOrderer { get; set; }
		public required ITestClassOrderer? TestClassOrderer { get; set; }
		public ICodeGenTestCollectionFactory TestCollectionFactory { get; }
		public required ITestCollectionOrderer? TestCollectionOrderer { get; set; }
		public required ITestMethodOrderer? TestMethodOrderer { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }
		public required Version Version { get; set; }
	}

	public static ICodeGenTestCase CodeGenTestCase(
		Func<ICodeGenTestCase, IReadOnlyCollection<ICodeGenTest>>? createTests = null,
		bool @explicit = false,
		Action? postInvoke = null,
		Action? preInvoke = null,
		Type[]? skipExceptions = null,
		string? skipReason = null,
		Func<bool>? skipUnless = null,
		Func<bool>? skipWhen = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = TestData.DefaultTestCaseDisplayName,
		int testClassMetadataToken = TestData.DefaultTestClassMetadataToken,
		ICodeGenTestMethod? testMethod = null,
		int testMethodMetadataToken = TestData.DefaultTestMethodMetadataToken,
		string[]? testMethodParameterTypesVSTest = null,
		string testMethodReturnTypeVSTest = "System.Void",
		int timeout = 0,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestCaseUniqueID) =>
			new MockCodeGenTestCase(createTests ?? (tc => [CodeGenTest(testCase: tc)]), preInvoke, postInvoke)
			{
				Explicit = @explicit,
				SkipExceptions = skipExceptions,
				SkipReason = skipReason,
				SkipUnless = skipUnless,
				SkipWhen = skipWhen,
				SourceFilePath = sourceFilePath,
				SourceLineNumber = sourceLineNumber,
				TestCaseDisplayName = testCaseDisplayName,
				TestClassMetadataToken = testClassMetadataToken,
				TestMethod = testMethod ?? CodeGenTestMethod(),
				TestMethodMetadataToken = testMethodMetadataToken,
				TestMethodParameterTypesVSTest = testMethodParameterTypesVSTest,
				TestMethodReturnTypeVSTest = testMethodReturnTypeVSTest,
				Timeout = timeout,
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockCodeGenTestCase(
		Func<ICodeGenTestCase, IReadOnlyCollection<ICodeGenTest>> createTests,
		Action? preInvoke,
		Action? postInvoke) :
			ICodeGenTestCase
	{
		public required bool Explicit { get; set; }
		public required Type[]? SkipExceptions { get; set; }
		public required string? SkipReason { get; set; }
		public required Func<bool>? SkipUnless { get; set; }
		public required Func<bool>? SkipWhen { get; set; }
		public required string? SourceFilePath { get; set; }
		public required int? SourceLineNumber { get; set; }
		public required string TestCaseDisplayName { get; set; }
		public required int? TestClassMetadataToken { get; set; }
		public required ICodeGenTestMethod TestMethod { get; set; }
		public required int? TestMethodMetadataToken { get; set; }
		public required string[]? TestMethodParameterTypesVSTest { get; set; }
		public required string? TestMethodReturnTypeVSTest { get; set; }
		public required int Timeout { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		public ICodeGenTestClass TestClass => TestMethod.TestClass;
		ITestClass? ITestCase.TestClass => TestClass;
		ICoreTestClass ICoreTestCase.TestClass => TestClass;
		public string TestClassName => TestClass.TestClassName;
		public string? TestClassNamespace => TestClass.TestClassNamespace;
		public string TestClassSimpleName => TestClass.TestClassSimpleName;
		public ICodeGenTestCollection TestCollection => TestClass.TestCollection;
		ITestCollection ITestCase.TestCollection => TestCollection;
		ICoreTestCollection ICoreTestCase.TestCollection => TestCollection;
		ITestMethod? ITestCase.TestMethod => TestMethod;
		ICoreTestMethod ICoreTestCase.TestMethod => TestMethod;
		public int TestMethodArity => TestMethod.MethodArity;
		int? ITestCaseMetadata.TestMethodArity => TestMethodArity;
		public string TestMethodName => TestMethod.MethodName;

		public ValueTask<IReadOnlyCollection<ICodeGenTest>> CreateTests() => new(createTests(this));
		public void PostInvoke() => postInvoke?.Invoke();
		public void PreInvoke() => preInvoke?.Invoke();
	}

	public static ICodeGenTestClass CodeGenTestClass(
		IReadOnlyCollection<BeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>? classFixtureFactories = null,
		ITestCaseOrderer? testCaseOrderer = null,
		Func<FixtureMappingManager, ValueTask<CoreTestClassCreationResult>>? testClassFactory = null,
		string testClassName = TestData.DefaultTestClassName,
		string testClassNamespace = TestData.DefaultTestClassNamespace,
		string testClassSimpleName = TestData.DefaultTestClassSimpleName,
		ICodeGenTestCollection? testCollection = null,
		ITestMethodOrderer? testMethodOrderer = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestClassUniqueID) =>
			new MockCodeGenTestClass
			{
				BeforeAfterTestAttributes = beforeAfterTestAttributes ?? [],
				ClassFixtureFactories = classFixtureFactories ?? TestData.EmptyFixtureFactories,
				TestCaseOrderer = testCaseOrderer,
				TestClassFactory = testClassFactory ?? TestData.DefaultClassFactory,
				TestClassName = testClassName,
				TestClassNamespace = testClassNamespace,
				TestClassSimpleName = testClassSimpleName,
				TestCollection = testCollection ?? CodeGenTestCollection(),
				TestMethodOrderer = testMethodOrderer,
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	public static ICodeGenTestClass CodeGenTestClass<TClass>(
		IReadOnlyCollection<BeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>? classFixtureFactories = null,
		ITestCaseOrderer? testCaseOrderer = null,
		string testClassName = TestData.DefaultTestClassName,
		string testClassNamespace = TestData.DefaultTestClassNamespace,
		string testClassSimpleName = TestData.DefaultTestClassSimpleName,
		ICodeGenTestCollection? testCollection = null,
		ITestMethodOrderer? testMethodOrderer = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestClassUniqueID)
			where TClass : new() =>
				new MockCodeGenTestClass
				{
					BeforeAfterTestAttributes = beforeAfterTestAttributes ?? [],
					ClassFixtureFactories = classFixtureFactories ?? TestData.EmptyFixtureFactories,
					TestCaseOrderer = testCaseOrderer,
					TestClassFactory = _ => new(new CoreTestClassCreationResult(new TClass())),
					TestClassName = testClassName,
					TestClassNamespace = testClassNamespace,
					TestClassSimpleName = testClassSimpleName,
					TestCollection = testCollection ?? CodeGenTestCollection(),
					TestMethodOrderer = testMethodOrderer,
					Traits = traits ?? TestData.DefaultTraits,
					UniqueID = uniqueID,
				};

	class MockCodeGenTestClass : ICodeGenTestClass
	{
		public required IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; set; }
		public required IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> ClassFixtureFactories { get; set; }
		public required ITestCaseOrderer? TestCaseOrderer { get; set; }
		public required Func<FixtureMappingManager, ValueTask<CoreTestClassCreationResult>> TestClassFactory { get; set; }
		public required string TestClassName { get; set; }
		public required string? TestClassNamespace { get; set; }
		public required string TestClassSimpleName { get; set; }
		public required ICodeGenTestCollection TestCollection { get; set; }
		public required ITestMethodOrderer? TestMethodOrderer { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		public Type Class => throw new NotSupportedException("Using ICodeGenTestClass.Class while testing is prohibited");

		ITestCollection ITestClass.TestCollection => TestCollection;
		ICoreTestCollection ICoreTestClass.TestCollection => TestCollection;
	}

	public static ICodeGenTestCollection CodeGenTestCollection(
		IReadOnlyCollection<BeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>? classFixtureFactories = null,
		IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>>? collectionFixtureFactories = null,
		bool disableParallelization = false,
		ICodeGenTestAssembly? testAssembly = null,
		ITestCaseOrderer? testCaseOrderer = null,
		ITestClassOrderer? testClassOrderer = null,
		string? testCollectionClassName = null,
		string testCollectionDisplayName = TestData.DefaultTestCollectionDisplayName,
		ITestMethodOrderer? testMethodOrderer = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestCollectionUniqueID) =>
			new MockCodeGenTestCollection
			{
				BeforeAfterTestAttributes = beforeAfterTestAttributes ?? [],
				ClassFixtureFactories = classFixtureFactories ?? TestData.EmptyFixtureFactories,
				CollectionFixtureFactories = collectionFixtureFactories ?? TestData.EmptyFixtureFactories,
				DisableParallelization = disableParallelization,
				TestAssembly = testAssembly ?? CodeGenTestAssembly(),
				TestCaseOrderer = testCaseOrderer,
				TestClassOrderer = testClassOrderer,
				TestCollectionClassName = testCollectionClassName,
				TestCollectionDisplayName = testCollectionDisplayName,
				TestMethodOrderer = testMethodOrderer,
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockCodeGenTestCollection : ICodeGenTestCollection
	{
		public required IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; set; }
		public required IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> ClassFixtureFactories { get; set; }
		public required IReadOnlyDictionary<Type, Func<FixtureMappingManager?, ValueTask<object>>> CollectionFixtureFactories { get; set; }
		public required bool DisableParallelization { get; set; }
		public required ICodeGenTestAssembly TestAssembly { get; set; }
		public required ITestCaseOrderer? TestCaseOrderer { get; set; }
		public required ITestClassOrderer? TestClassOrderer { get; set; }
		public required string? TestCollectionClassName { get; set; }
		public required string TestCollectionDisplayName { get; set; }
		public required ITestMethodOrderer? TestMethodOrderer { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		ITestAssembly ITestCollection.TestAssembly => TestAssembly;
		ICoreTestAssembly ICoreTestCollection.TestAssembly => TestAssembly;
	}

	public static ICodeGenTestMethod CodeGenTestMethod(
		IReadOnlyCollection<BeforeAfterTestAttribute>? beforeAfterTestAttributes = null,
		string? declaredTypeIndex = null,
		bool isStatic = false,
		int methodArity = 0,
		string methodName = TestData.DefaultMethodName,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		ITestCaseOrderer? testCaseOrderer = null,
		ICodeGenTestClass? testClass = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = TestData.DefaultTestMethodUniqueID) =>
			new MockCodeGenTestMethod
			{
				BeforeAfterTestAttributes = beforeAfterTestAttributes ?? [],
				DeclaredTypeIndex = declaredTypeIndex,
				IsStatic = isStatic,
				MethodArity = methodArity,
				MethodName = methodName,
				SourceFilePath = sourceFilePath,
				SourceLineNumber = sourceLineNumber,
				TestCaseOrderer = testCaseOrderer,
				TestClass = testClass ?? CodeGenTestClass(),
				Traits = traits ?? TestData.DefaultTraits,
				UniqueID = uniqueID,
			};

	class MockCodeGenTestMethod : ICodeGenTestMethod
	{
		public required IReadOnlyCollection<BeforeAfterTestAttribute> BeforeAfterTestAttributes { get; set; }
		public required string? DeclaredTypeIndex { get; set; }
		public required bool IsStatic { get; set; }
		public required int MethodArity { get; set; }
		public required string MethodName { get; set; }
		public required string? SourceFilePath { get; set; }
		public required int? SourceLineNumber { get; set; }
		public required ITestCaseOrderer? TestCaseOrderer { get; set; }
		public required ICodeGenTestClass TestClass { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }

		ITestClass ITestMethod.TestClass => TestClass;
		ICoreTestClass ICoreTestMethod.TestClass => TestClass;
		int? ITestMethodMetadata.MethodArity => MethodArity;
	}
}
