using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

// This file manufactures instances of the test messages
public static partial class TestData
{
	public static AfterTestFinished AfterTestFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string attributeName = DefaultAttributeName,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				AttributeName = attributeName,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static AfterTestStarting AfterTestStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string attributeName = DefaultAttributeName,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				AttributeName = attributeName,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static BeforeTestFinished BeforeTestFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string attributeName = DefaultAttributeName,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				AttributeName = attributeName,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static BeforeTestStarting BeforeTestStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string attributeName = DefaultAttributeName,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				AttributeName = attributeName,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static DiagnosticMessage DiagnosticMessage(string message = "Hello world!") =>
		new(message);

	public static DiscoveryComplete DiscoveryComplete(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		int testCasesToRun = DefaultCountTotal) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCasesToRun = testCasesToRun,
			};

	public static DiscoveryStarting DiscoveryStarting(
		string assemblyName = DefaultAssemblyName,
		string assemblyPath = DefaultAssemblyPath,
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string? configFilePath = DefaultConfigFilePath) =>
			new()
			{
				AssemblyName = assemblyName,
				AssemblyPath = assemblyPath,
				AssemblyUniqueID = assemblyUniqueID,
				ConfigFilePath = configFilePath,
			};

	public static ErrorMessage ErrorMessage(
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		string[]? messages = null,
		string?[]? stackTraces = null) =>
			new()
			{
				ExceptionParentIndices = exceptionParentIndices ?? DefaultExceptionParentIndices,
				ExceptionTypes = exceptionTypes ?? DefaultExceptionTypes,
				Messages = messages ?? DefaultExceptionMessages,
				StackTraces = stackTraces ?? DefaultStackTraces,
			};

	public static InternalDiagnosticMessage InternalDiagnosticMessage(string message = "Hello world!") =>
		new(message);

	public static TestAssemblyCleanupFailure TestAssemblyCleanupFailure(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		string[]? messages = null,
		string?[]? stackTraces = null) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExceptionParentIndices = exceptionParentIndices ?? DefaultExceptionParentIndices,
				ExceptionTypes = exceptionTypes ?? DefaultExceptionTypes,
				Messages = messages ?? DefaultExceptionMessages,
				StackTraces = stackTraces ?? DefaultStackTraces,
			};

	public static TestAssemblyFinished TestAssemblyFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = DefaultExecutionTime,
		DateTimeOffset? finishTime = null,
		int testsFailed = DefaultCountFailed,
		int testsNotRun = DefaultCountNotRun,
		int testsSkipped = DefaultCountSkipped,
		int testsTotal = DefaultCountTotal) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = executionTime,
				FinishTime = finishTime ?? DefaultFinishTime,
				TestsFailed = testsFailed,
				TestsNotRun = testsNotRun,
				TestsSkipped = testsSkipped,
				TestsTotal = testsTotal,
			};

	public static TestAssemblyStarting TestAssemblyStarting(
		string assemblyName = DefaultAssemblyName,
		string assemblyPath = DefaultAssemblyPath,
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string? configFilePath = DefaultConfigFilePath,
		int? seed = null,
		DateTimeOffset? startTime = null,
		string? targetFramework = DefaultTargetFramework,
		string testEnvironment = DefaultTestEnvironment,
		string testFrameworkDisplayName = DefaultTestFrameworkDisplayName,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null) =>
			new()
			{
				AssemblyName = assemblyName,
				AssemblyPath = assemblyPath,
				AssemblyUniqueID = assemblyUniqueID,
				ConfigFilePath = configFilePath,
				Seed = seed,
				StartTime = startTime ?? new DateTimeOffset(2021, 1, 20, 17, 0, 0, TimeSpan.Zero),
				TargetFramework = targetFramework,
				TestEnvironment = testEnvironment,
				TestFrameworkDisplayName = testFrameworkDisplayName,
				Traits = traits ?? DefaultTraits,
			};

	public static TestAssemblyDiscoveryFinished TestAssemblyDiscoveryFinished(
		string assemblyPath = DefaultAssemblyPath,
		string configFilePath = DefaultConfigFilePath,
		AssemblyMetadata? assemblyMetadata = null,
		bool diagnosticMessages = false,
		bool internalDiagnosticMessages = false,
		TestMethodDisplay methodDisplay = TestMethodDisplay.ClassAndMethod,
		TestMethodDisplayOptions methodDisplayOptions = TestMethodDisplayOptions.None,
		bool preEnumerateTheories = false,
		int testCasesToRun = 42)
	{
		var project = new XunitProject();
		var assembly = new XunitProjectAssembly(project, assemblyPath, assemblyMetadata ?? new(3, DefaultTargetFramework)) { ConfigFileName = configFilePath };
		var discoveryOptions = TestFrameworkDiscoveryOptions(
			diagnosticMessages: diagnosticMessages,
			internalDiagnosticMessages: internalDiagnosticMessages,
			methodDisplay: methodDisplay,
			methodDisplayOptions: methodDisplayOptions,
			preEnumerateTheories: preEnumerateTheories
		);

		return new()
		{
			Assembly = assembly,
			DiscoveryOptions = discoveryOptions,
			TestCasesToRun = testCasesToRun,
		};
	}

	public static TestAssemblyDiscoveryStarting TestAssemblyDiscoveryStarting(
		AppDomainOption appDomain = AppDomainOption.Disabled,
		string assemblyPath = DefaultAssemblyPath,
		string configFilePath = DefaultConfigFilePath,
		AssemblyMetadata? assemblyMetadata = null,
		bool diagnosticMessages = false,
		bool internalDiagnosticMessages = false,
		TestMethodDisplay methodDisplay = TestMethodDisplay.ClassAndMethod,
		TestMethodDisplayOptions methodDisplayOptions = TestMethodDisplayOptions.None,
		bool preEnumerateTheories = false,
		bool shadowCopy = false)
	{
		var project = new XunitProject();
		var assembly = new XunitProjectAssembly(project, assemblyPath, assemblyMetadata ?? new(3, DefaultTargetFramework)) { ConfigFileName = configFilePath };
		var discoveryOptions = TestFrameworkDiscoveryOptions(
			diagnosticMessages: diagnosticMessages,
			internalDiagnosticMessages: internalDiagnosticMessages,
			methodDisplay: methodDisplay,
			methodDisplayOptions: methodDisplayOptions,
			preEnumerateTheories: preEnumerateTheories
		);

		return new()
		{
			AppDomain = appDomain,
			Assembly = assembly,
			DiscoveryOptions = discoveryOptions,
			ShadowCopy = shadowCopy,
		};
	}

	public static TestAssemblyExecutionFinished TestAssemblyExecutionFinished(
		string assemblyPath = DefaultAssemblyPath,
		string configFilePath = DefaultConfigFilePath,
		AssemblyMetadata? assemblyMetadata = null,
		bool diagnosticMessages = false,
		decimal executionTime = 123.4567m,
		bool internalDiagnosticMessages = false,
		int maxParallelThreads = 2600,
		bool parallelizeTestCollections = false,
		int testsErrored = 95,
		int testsFailed = DefaultCountFailed,
		int testsNotRun = DefaultCountNotRun,
		int testsSkipped = DefaultCountSkipped,
		int testsTotal = DefaultCountTotal)
	{
		var project = new XunitProject();
		var assembly = new XunitProjectAssembly(project, assemblyPath, assemblyMetadata ?? new(3, DefaultTargetFramework)) { ConfigFileName = configFilePath };
		// See the ForExecution method to see which TestAssemblyConfiguration options are used for discovery
		var executionOptions = TestFrameworkExecutionOptions(
			diagnosticMessages: diagnosticMessages,
			disableParallelization: !parallelizeTestCollections,
			internalDiagnosticMessages: internalDiagnosticMessages,
			maxParallelThreads: maxParallelThreads
		);
		var executionSummary = new ExecutionSummary
		{
			Errors = testsErrored,
			Failed = testsFailed,
			NotRun = testsNotRun,
			Skipped = testsSkipped,
			Time = executionTime,
			Total = testsTotal,
		};

		return new()
		{
			Assembly = assembly,
			ExecutionOptions = executionOptions,
			ExecutionSummary = executionSummary,
		};
	}

	public static TestAssemblyExecutionStarting TestAssemblyExecutionStarting(
		string assemblyPath = DefaultAssemblyPath,
		string configFilePath = DefaultConfigFilePath,
		AssemblyMetadata? assemblyMetadata = null,
		bool diagnosticMessages = false,
		ExplicitOption? explicitOption = null,
		bool internalDiagnosticMessages = false,
		int maxParallelThreads = 2600,
		ParallelAlgorithm? parallelAlgorithm = null,
		bool? parallelizeTestCollections = null,
		bool? stopOnFail = null,
		int? seed = null,
		string? culture = null,
		bool? showLiveOutput = null)
	{
		var project = new XunitProject();
		var assembly = new XunitProjectAssembly(project, assemblyPath, assemblyMetadata ?? new(3, DefaultTargetFramework)) { ConfigFileName = configFilePath };
		// See the ForExecution method to see which TestAssemblyConfiguration options are used for discovery
		var executionOptions = TestFrameworkExecutionOptions(
			culture: culture,
			diagnosticMessages: diagnosticMessages,
			disableParallelization: !parallelizeTestCollections,
			explicitOption: explicitOption,
			internalDiagnosticMessages: internalDiagnosticMessages,
			maxParallelThreads: maxParallelThreads,
			parallelAlgorithm: parallelAlgorithm,
			showLiveOutput: showLiveOutput,
			stopOnFail: stopOnFail
		);

		return new()
		{
			Assembly = assembly,
			ExecutionOptions = executionOptions,
			Seed = seed,
		};
	}

	public static TestCaseCleanupFailure TestCaseCleanupFailure(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		string[]? messages = null,
		string?[]? stackTraces = null,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExceptionParentIndices = exceptionParentIndices ?? DefaultExceptionParentIndices,
				ExceptionTypes = exceptionTypes ?? DefaultExceptionTypes,
				Messages = messages ?? DefaultExceptionMessages,
				StackTraces = stackTraces ?? DefaultStackTraces,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
			};

	public static TestCaseDiscovered TestCaseDiscovered<TClass>(
		string testMethod,
		string? testCaseDisplayName = null)
	{
		var typeInfo = typeof(TClass);
		var methodInfo = Guard.NotNull($"Could not find method '{testMethod}' in type '{typeInfo.FullName}'", typeInfo.GetMethod(testMethod));
		var factAttribute = methodInfo.GetMatchingCustomAttributes(typeof(IFactAttribute)).FirstOrDefault() as IFactAttribute;
		var skipReason = factAttribute?.Skip;
		var traits = ExtensibilityPointFactory.GetMethodTraits(methodInfo, testClassTraits: null);

		var testClassUniqueID = UniqueIDGenerator.ForTestClass(DefaultTestCollectionUniqueID, typeInfo.FullName);
		var testMethodUniqueID = UniqueIDGenerator.ForTestMethod(testClassUniqueID, testMethod);
		var testCaseUniqueID = UniqueIDGenerator.ForTestCase(testMethodUniqueID, null, null);

		return TestCaseDiscovered(
			DefaultAssemblyUniqueID,
			DefaultTestCaseSerialization,
			skipReason,
			sourceFilePath: null,
			sourceLineNumber: null,
			testCaseDisplayName ?? $"{typeInfo.FullName}.{testMethod}",
			testCaseUniqueID,
			typeInfo.FullName,
			testClassUniqueID,
			DefaultTestCollectionUniqueID,
			testMethod,
			testMethodUniqueID,
			typeInfo.Namespace,
			traits
		);
	}

	public static TestCaseDiscovered TestCaseDiscovered(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string serialization = DefaultTestCaseSerialization,
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = DefaultTestCaseDisplayName,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassName = null,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethod = null,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string? testNamespace = null,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				Serialization = serialization,
				SkipReason = skipReason,
				SourceFilePath = sourceFilePath,
				SourceLineNumber = sourceLineNumber,
				TestCaseDisplayName = testCaseDisplayName,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassName = testClassName,
				TestClassNamespace = testNamespace,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodName = testMethod,
				TestMethodUniqueID = testMethodUniqueID,
				Traits = traits ?? DefaultTraits,
			};

	public static TestCaseFinished TestCaseFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = 123.4567m,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		int testsFailed = DefaultCountFailed,
		int testsNotRun = DefaultCountNotRun,
		int testsSkipped = DefaultCountSkipped,
		int testsTotal = DefaultCountTotal) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = executionTime,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestsFailed = testsFailed,
				TestsNotRun = testsNotRun,
				TestsSkipped = testsSkipped,
				TestsTotal = testsTotal,
			};

	public static TestCaseStarting TestCaseStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = DefaultTestCaseDisplayName,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string testClassName = DefaultTestClassName,
		string testClassNamespace = DefaultTestClassNamespace,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string testMethodName = DefaultMethodName,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				SkipReason = skipReason,
				SourceFilePath = sourceFilePath,
				SourceLineNumber = sourceLineNumber,
				TestCaseDisplayName = testCaseDisplayName,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassName = testClassName,
				TestClassNamespace = testClassNamespace,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodName = testMethodName,
				TestMethodUniqueID = testMethodUniqueID,
				Traits = traits ?? DefaultTraits,
			};

	public static TestClassCleanupFailure TestClassCleanupFailure(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		string[]? messages = null,
		string?[]? stackTraces = null,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExceptionParentIndices = exceptionParentIndices ?? DefaultExceptionParentIndices,
				ExceptionTypes = exceptionTypes ?? DefaultExceptionTypes,
				Messages = messages ?? DefaultExceptionMessages,
				StackTraces = stackTraces ?? DefaultStackTraces,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
			};

	public static TestClassConstructionFinished TestClassConstructionFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static TestClassConstructionStarting TestClassConstructionStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static TestClassDisposeFinished TestClassDisposeFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static TestClassDisposeStarting TestClassDisposeStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static TestClassFinished TestClassFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = DefaultExecutionTime,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		int testsFailed = DefaultCountFailed,
		int testsNotRun = DefaultCountNotRun,
		int testsSkipped = DefaultCountSkipped,
		int testsTotal = DefaultCountTotal) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = executionTime,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestsFailed = testsFailed,
				TestsNotRun = testsNotRun,
				TestsSkipped = testsSkipped,
				TestsTotal = testsTotal,
			};

	public static TestClassStarting TestClassStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string testClassName = DefaultTestClassName,
		string testClassNamespace = DefaultTestClassNamespace,
		string testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestClassName = testClassName,
				TestClassNamespace = testClassNamespace,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				Traits = traits ?? DefaultTraits,
			};

	public static TestCleanupFailure TestCleanupFailure(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		string[]? messages = null,
		string?[]? stackTraces = null,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExceptionParentIndices = exceptionParentIndices ?? DefaultExceptionParentIndices,
				ExceptionTypes = exceptionTypes ?? DefaultExceptionTypes,
				Messages = messages ?? DefaultExceptionMessages,
				StackTraces = stackTraces ?? DefaultStackTraces,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static TestCollectionCleanupFailure TestCollectionCleanupFailure(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		string[]? messages = null,
		string?[]? stackTraces = null,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExceptionParentIndices = exceptionParentIndices ?? DefaultExceptionParentIndices,
				ExceptionTypes = exceptionTypes ?? DefaultExceptionTypes,
				Messages = messages ?? DefaultExceptionMessages,
				StackTraces = stackTraces ?? DefaultStackTraces,
				TestCollectionUniqueID = testCollectionUniqueID,
			};

	public static TestCollectionFinished TestCollectionFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = DefaultExecutionTime,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		int testsFailed = DefaultCountFailed,
		int testsNotRun = DefaultCountNotRun,
		int testsSkipped = DefaultCountSkipped,
		int testsTotal = DefaultCountTotal) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = executionTime,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestsFailed = testsFailed,
				TestsNotRun = testsNotRun,
				TestsSkipped = testsSkipped,
				TestsTotal = testsTotal,
			};

	public static TestCollectionStarting TestCollectionStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string? testCollectionClass = DefaultTestCollectionClass,
		string testCollectionDisplayName = DefaultTestCollectionDisplayName,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCollectionClassName = testCollectionClass,
				TestCollectionDisplayName = testCollectionDisplayName,
				TestCollectionUniqueID = testCollectionUniqueID,
				Traits = traits ?? DefaultTraits,
			};

	public static TestExecutionSummaries TestExecutionSummaries(
		TimeSpan clockTime,
		string assemblyUniqueID,
		ExecutionSummary summary) =>
			TestExecutionSummaries(clockTime, (assemblyUniqueID, summary));

	public static TestExecutionSummaries TestExecutionSummaries(
		TimeSpan clockTime,
		params (string assemblyUniqueID, ExecutionSummary summary)[] summaries)
	{
		var result = new TestExecutionSummaries { ElapsedClockTime = clockTime };
		foreach (var summary in summaries)
			result.Add(summary.assemblyUniqueID, summary.summary);

		return result;
	}

	public static TestFailed TestFailed(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		FailureCause cause = FailureCause.Assertion,
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		decimal executionTime = DefaultExecutionTime,
		string[]? messages = null,
		string output = DefaultOutput,
		string?[]? stackTraces = null,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID,
		string[]? warnings = null) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				Cause = cause,
				ExceptionParentIndices = exceptionParentIndices ?? DefaultExceptionParentIndices,
				ExceptionTypes = exceptionTypes ?? DefaultExceptionTypes,
				ExecutionTime = executionTime,
				Messages = messages ?? DefaultExceptionMessages,
				Output = output,
				StackTraces = stackTraces ?? DefaultStackTraces,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
				Warnings = warnings,
			};

	public static TestFinished TestFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = DefaultExecutionTime,
		string output = DefaultOutput,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID,
		string[]? warnings = null) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = executionTime,
				Output = output,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
				Warnings = warnings,
			};

	public static TestMethodCleanupFailure TestMethodCleanupFailure(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		string[]? messages = null,
		string?[]? stackTraces = null,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExceptionParentIndices = exceptionParentIndices ?? DefaultExceptionParentIndices,
				ExceptionTypes = exceptionTypes ?? DefaultExceptionTypes,
				Messages = messages ?? DefaultExceptionMessages,
				StackTraces = stackTraces ?? DefaultStackTraces,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
			};

	public static TestMethodFinished TestMethodFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = DefaultExecutionTime,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		int testsFailed = DefaultCountFailed,
		int testsNotRun = DefaultCountNotRun,
		int testsSkipped = DefaultCountSkipped,
		int testsTotal = DefaultCountTotal) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = executionTime,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestsFailed = testsFailed,
				TestsNotRun = testsNotRun,
				TestsSkipped = testsSkipped,
				TestsTotal = testsTotal,
			};

	public static TestMethodStarting TestMethodStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string methodName = DefaultMethodName,
		string testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string testMethodUniqueID = DefaultTestMethodUniqueID,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				MethodName = methodName,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				Traits = traits ?? DefaultTraits,
			};

	public static TestNotRun TestNotRun(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string output = DefaultOutput,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID,
		string[]? warnings = null) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = 0m,
				Output = output,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
				Warnings = warnings,
			};

	public static TestOutput TestOutput(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string output = DefaultOutput,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				Output = output,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static TestPassed TestPassed(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = DefaultExecutionTime,
		string output = DefaultOutput,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID,
		string[]? warnings = null) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = executionTime,
				Output = output,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
				Warnings = warnings,
			};

	public static TestSkipped TestSkipped(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string output = DefaultOutput,
		string reason = DefaultSkipReason,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID,
		string[]? warnings = null) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = 0m,
				Output = output,
				Reason = reason,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
				Warnings = warnings,
			};

	public static TestStarting TestStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		bool @explicit = false,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string testDisplayName = DefaultTestDisplayName,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID,
		int timeout = 0,
		IReadOnlyDictionary<string, IReadOnlyList<string>>? traits = null) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				Explicit = @explicit,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestDisplayName = testDisplayName,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
				Timeout = timeout,
				Traits = traits ?? DefaultTraits,
			};
}
