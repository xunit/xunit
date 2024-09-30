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
	public static IAfterTestFinished AfterTestFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string attributeName = DefaultAttributeName,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new Xunit.Runner.Common.AfterTestFinished()
			{
				AssemblyUniqueID = assemblyUniqueID,
				AttributeName = attributeName,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static IAfterTestStarting AfterTestStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string attributeName = DefaultAttributeName,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new Xunit.Runner.Common.AfterTestStarting()
			{
				AssemblyUniqueID = assemblyUniqueID,
				AttributeName = attributeName,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static IBeforeTestFinished BeforeTestFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string attributeName = DefaultAttributeName,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new Xunit.Runner.Common.BeforeTestFinished()
			{
				AssemblyUniqueID = assemblyUniqueID,
				AttributeName = attributeName,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static IBeforeTestStarting BeforeTestStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string attributeName = DefaultAttributeName,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new Xunit.Runner.Common.BeforeTestStarting()
			{
				AssemblyUniqueID = assemblyUniqueID,
				AttributeName = attributeName,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static IDiagnosticMessage DiagnosticMessage(string message = "Hello world!") =>
		new Xunit.Runner.Common.DiagnosticMessage(message);

	public static IDiscoveryComplete DiscoveryComplete(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		int testCasesToRun = DefaultCountTotal) =>
			new Xunit.Runner.Common.DiscoveryComplete()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCasesToRun = testCasesToRun,
			};

	public static IDiscoveryStarting DiscoveryStarting(
		string assemblyName = DefaultAssemblyName,
		string assemblyPath = DefaultAssemblyPath,
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string? configFilePath = DefaultConfigFilePath) =>
			new Xunit.Runner.Common.DiscoveryStarting()
			{
				AssemblyName = assemblyName,
				AssemblyPath = assemblyPath,
				AssemblyUniqueID = assemblyUniqueID,
				ConfigFilePath = configFilePath,
			};

	public static IErrorMessage ErrorMessage(
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		string[]? messages = null,
		string?[]? stackTraces = null) =>
			new Xunit.Runner.Common.ErrorMessage()
			{
				ExceptionParentIndices = exceptionParentIndices ?? DefaultExceptionParentIndices,
				ExceptionTypes = exceptionTypes ?? DefaultExceptionTypes,
				Messages = messages ?? DefaultExceptionMessages,
				StackTraces = stackTraces ?? DefaultStackTraces,
			};

	public static IInternalDiagnosticMessage InternalDiagnosticMessage(string message = "Hello world!") =>
		new Xunit.Runner.Common.InternalDiagnosticMessage(message);

	public static ITestAssemblyCleanupFailure TestAssemblyCleanupFailure(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		string[]? messages = null,
		string?[]? stackTraces = null) =>
			new Xunit.Runner.Common.TestAssemblyCleanupFailure()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExceptionParentIndices = exceptionParentIndices ?? DefaultExceptionParentIndices,
				ExceptionTypes = exceptionTypes ?? DefaultExceptionTypes,
				Messages = messages ?? DefaultExceptionMessages,
				StackTraces = stackTraces ?? DefaultStackTraces,
			};

	public static ITestAssemblyFinished TestAssemblyFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = DefaultExecutionTime,
		DateTimeOffset? finishTime = null,
		int testsFailed = DefaultCountFailed,
		int testsNotRun = DefaultCountNotRun,
		int testsSkipped = DefaultCountSkipped,
		int testsTotal = DefaultCountTotal) =>
			new Xunit.Runner.Common.TestAssemblyFinished()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = executionTime,
				FinishTime = finishTime ?? DefaultFinishTime,
				TestsFailed = testsFailed,
				TestsNotRun = testsNotRun,
				TestsSkipped = testsSkipped,
				TestsTotal = testsTotal,
			};

	public static ITestAssemblyStarting TestAssemblyStarting(
		string assemblyName = DefaultAssemblyName,
		string assemblyPath = DefaultAssemblyPath,
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string? configFilePath = DefaultConfigFilePath,
		int? seed = null,
		DateTimeOffset? startTime = null,
		string? targetFramework = DefaultTargetFramework,
		string testEnvironment = DefaultTestEnvironment,
		string testFrameworkDisplayName = DefaultTestFrameworkDisplayName,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null) =>
			new Xunit.Runner.Common.TestAssemblyStarting()
			{
				AssemblyName = assemblyName,
				AssemblyPath = assemblyPath,
				AssemblyUniqueID = assemblyUniqueID,
				ConfigFilePath = configFilePath,
				Seed = seed,
				StartTime = startTime ?? DefaultStartTime,
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

	public static ITestCaseCleanupFailure TestCaseCleanupFailure(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		string[]? messages = null,
		string?[]? stackTraces = null,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID) =>
			new Xunit.Runner.Common.TestCaseCleanupFailure()
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

	public static ITestCaseDiscovered TestCaseDiscovered<TClass>(
		string testMethod,
		string? testCaseDisplayName = null)
	{
		var typeInfo = typeof(TClass);
		var methodInfo = Guard.NotNull($"Could not find method '{testMethod}' in type '{typeInfo.FullName}'", typeInfo.GetMethod(testMethod));
		var factAttribute = methodInfo.GetMatchingCustomAttributes(typeof(IFactAttribute)).FirstOrDefault() as IFactAttribute;
		var @explicit = factAttribute?.Explicit ?? false;
		var skipReason = factAttribute?.Skip;
		var traits = ExtensibilityPointFactory.GetMethodTraits(methodInfo, testClassTraits: null);

		var testClassUniqueID = UniqueIDGenerator.ForTestClass(DefaultTestCollectionUniqueID, typeInfo.FullName);
		var testMethodUniqueID = UniqueIDGenerator.ForTestMethod(testClassUniqueID, testMethod);
		var testCaseUniqueID = UniqueIDGenerator.ForTestCase(testMethodUniqueID, null, null);

		return TestCaseDiscovered(
			DefaultAssemblyUniqueID,
			@explicit,
			DefaultTestCaseSerialization,
			skipReason,
			sourceFilePath: null,
			sourceLineNumber: null,
			testCaseDisplayName ?? $"{typeInfo.FullName}.{testMethod}",
			testCaseUniqueID,
			typeInfo.MetadataToken,
			typeInfo.FullName,
			typeInfo.Namespace,
			typeInfo.Name,
			testClassUniqueID,
			DefaultTestCollectionUniqueID,
			methodInfo.MetadataToken,
			testMethod,
			methodInfo.GetParameters().Select(p => p.ParameterType.ToVSTestTypeName()).ToArray(),
			methodInfo.ReturnType.ToVSTestTypeName(),
			testMethodUniqueID,
			traits
		);
	}

	public static ITestCaseDiscovered TestCaseDiscovered(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		bool @explicit = false,
		string serialization = DefaultTestCaseSerialization,
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = DefaultTestCaseDisplayName,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		int? testClassMetadataToken = null,
		string? testClassName = null,
		string? testClassNamespace = null,
		string? testClassSimpleName = null,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		int? testMethodMetadataToken = null,
		string? testMethodName = null,
		string[]? testMethodParameterTypesVSTest = null,
		string? testMethodReturnTypeVSTest = null,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null) =>
			new Xunit.Runner.Common.TestCaseDiscovered()
			{
				AssemblyUniqueID = assemblyUniqueID,
				Explicit = @explicit,
				Serialization = serialization,
				SkipReason = skipReason,
				SourceFilePath = sourceFilePath,
				SourceLineNumber = sourceLineNumber,
				TestCaseDisplayName = testCaseDisplayName,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassMetadataToken = testClassMetadataToken,
				TestClassName = testClassName,
				TestClassNamespace = testClassNamespace,
				TestClassSimpleName = testClassSimpleName,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodMetadataToken = testMethodMetadataToken,
				TestMethodName = testMethodName,
				TestMethodParameterTypesVSTest = testMethodParameterTypesVSTest ?? [],
				TestMethodReturnTypeVSTest = testMethodReturnTypeVSTest ?? typeof(void).ToVSTestTypeName(),
				TestMethodUniqueID = testMethodUniqueID,
				Traits = traits ?? DefaultTraits,
			};

	public static ITestCaseFinished TestCaseFinished(
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
			new Xunit.Runner.Common.TestCaseFinished()
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

	public static ITestCaseStarting TestCaseStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		bool @explicit = false,
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = DefaultTestCaseDisplayName,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		int? testClassMetadataToken = 42,
		string testClassName = DefaultTestClassName,
		string testClassNamespace = DefaultTestClassNamespace,
		string testClassSimpleName = DefaultTestClassSimpleName,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		int? testMethodMetadataToken = 2112,
		string testMethodName = DefaultMethodName,
		string[]? testMethodParameterTypesVSTest = null,
		string? testMethodReturnTypeVSTest = null,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null) =>
			new Xunit.Runner.Common.TestCaseStarting()
			{
				AssemblyUniqueID = assemblyUniqueID,
				Explicit = @explicit,
				SkipReason = skipReason,
				SourceFilePath = sourceFilePath,
				SourceLineNumber = sourceLineNumber,
				TestCaseDisplayName = testCaseDisplayName,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassMetadataToken = testClassMetadataToken,
				TestClassName = testClassName,
				TestClassNamespace = testClassNamespace,
				TestClassSimpleName = testClassSimpleName,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodMetadataToken = testMethodMetadataToken,
				TestMethodName = testMethodName,
				TestMethodParameterTypesVSTest = testMethodParameterTypesVSTest ?? DefaultMethodParameterTypes,
				TestMethodReturnTypeVSTest = testMethodReturnTypeVSTest ?? DefaultMethodReturnType,
				TestMethodUniqueID = testMethodUniqueID,
				Traits = traits ?? DefaultTraits,
			};

	public static ITestClassCleanupFailure TestClassCleanupFailure(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		string[]? messages = null,
		string?[]? stackTraces = null,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID) =>
			new Xunit.Runner.Common.TestClassCleanupFailure()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExceptionParentIndices = exceptionParentIndices ?? DefaultExceptionParentIndices,
				ExceptionTypes = exceptionTypes ?? DefaultExceptionTypes,
				Messages = messages ?? DefaultExceptionMessages,
				StackTraces = stackTraces ?? DefaultStackTraces,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
			};

	public static ITestClassConstructionFinished TestClassConstructionFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new Xunit.Runner.Common.TestClassConstructionFinished()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static ITestClassConstructionStarting TestClassConstructionStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new Xunit.Runner.Common.TestClassConstructionStarting()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static ITestClassDisposeFinished TestClassDisposeFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new Xunit.Runner.Common.TestClassDisposeFinished()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static ITestClassDisposeStarting TestClassDisposeStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new Xunit.Runner.Common.TestClassDisposeStarting()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static ITestClassFinished TestClassFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = DefaultExecutionTime,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		int testsFailed = DefaultCountFailed,
		int testsNotRun = DefaultCountNotRun,
		int testsSkipped = DefaultCountSkipped,
		int testsTotal = DefaultCountTotal) =>
			new Xunit.Runner.Common.TestClassFinished()
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

	public static ITestClassStarting TestClassStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string testClassName = DefaultTestClassName,
		string testClassNamespace = DefaultTestClassNamespace,
		string testClassSimpleName = DefaultTestClassSimpleName,
		string testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null) =>
			new Xunit.Runner.Common.TestClassStarting()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestClassName = testClassName,
				TestClassNamespace = testClassNamespace,
				TestClassSimpleName = testClassSimpleName,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				Traits = traits ?? DefaultTraits,
			};

	public static ITestCleanupFailure TestCleanupFailure(
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
			new Xunit.Runner.Common.TestCleanupFailure()
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

	public static ITestCollectionCleanupFailure TestCollectionCleanupFailure(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		string[]? messages = null,
		string?[]? stackTraces = null,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID) =>
			new Xunit.Runner.Common.TestCollectionCleanupFailure()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExceptionParentIndices = exceptionParentIndices ?? DefaultExceptionParentIndices,
				ExceptionTypes = exceptionTypes ?? DefaultExceptionTypes,
				Messages = messages ?? DefaultExceptionMessages,
				StackTraces = stackTraces ?? DefaultStackTraces,
				TestCollectionUniqueID = testCollectionUniqueID,
			};

	public static ITestCollectionFinished TestCollectionFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = DefaultExecutionTime,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		int testsFailed = DefaultCountFailed,
		int testsNotRun = DefaultCountNotRun,
		int testsSkipped = DefaultCountSkipped,
		int testsTotal = DefaultCountTotal) =>
			new Xunit.Runner.Common.TestCollectionFinished()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = executionTime,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestsFailed = testsFailed,
				TestsNotRun = testsNotRun,
				TestsSkipped = testsSkipped,
				TestsTotal = testsTotal,
			};

	public static ITestCollectionStarting TestCollectionStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string? testCollectionClass = DefaultTestCollectionClass,
		string testCollectionDisplayName = DefaultTestCollectionDisplayName,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null) =>
			new Xunit.Runner.Common.TestCollectionStarting()
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

	public static ITestFailed TestFailed(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		FailureCause cause = FailureCause.Assertion,
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		decimal executionTime = DefaultExecutionTime,
		DateTimeOffset? finishTime = null,
		string[]? messages = null,
		string output = DefaultOutput,
		string?[]? stackTraces = null,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID,
		string[]? warnings = null) =>
			new Xunit.Runner.Common.TestFailed()
			{
				AssemblyUniqueID = assemblyUniqueID,
				Cause = cause,
				ExceptionParentIndices = exceptionParentIndices ?? DefaultExceptionParentIndices,
				ExceptionTypes = exceptionTypes ?? DefaultExceptionTypes,
				ExecutionTime = executionTime,
				FinishTime = finishTime ?? DefaultFinishTime,
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

	public static ITestFinished TestFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		IReadOnlyDictionary<string, TestAttachment>? attachments = null,
		decimal executionTime = DefaultExecutionTime,
		DateTimeOffset? finishTime = null,
		string output = DefaultOutput,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID,
		string[]? warnings = null) =>
			new Xunit.Runner.Common.TestFinished()
			{
				AssemblyUniqueID = assemblyUniqueID,
				Attachments = attachments ?? EmptyAttachments,
				ExecutionTime = executionTime,
				FinishTime = finishTime ?? DefaultFinishTime,
				Output = output,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
				Warnings = warnings,
			};

	public static ITestMethodCleanupFailure TestMethodCleanupFailure(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		string[]? messages = null,
		string?[]? stackTraces = null,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID) =>
			new Xunit.Runner.Common.TestMethodCleanupFailure()
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

	public static ITestMethodFinished TestMethodFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = DefaultExecutionTime,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		int testsFailed = DefaultCountFailed,
		int testsNotRun = DefaultCountNotRun,
		int testsSkipped = DefaultCountSkipped,
		int testsTotal = DefaultCountTotal) =>
			new Xunit.Runner.Common.TestMethodFinished()
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

	public static ITestMethodStarting TestMethodStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string methodName = DefaultMethodName,
		string testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string testMethodUniqueID = DefaultTestMethodUniqueID,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null) =>
			new Xunit.Runner.Common.TestMethodStarting()
			{
				AssemblyUniqueID = assemblyUniqueID,
				MethodName = methodName,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				Traits = traits ?? DefaultTraits,
			};

	public static ITestNotRun TestNotRun(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		DateTimeOffset? finishTime = null,
		string output = DefaultOutput,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID,
		string[]? warnings = null) =>
			new Xunit.Runner.Common.TestNotRun()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = 0m,
				FinishTime = finishTime ?? DefaultFinishTime,
				Output = output,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
				Warnings = warnings,
			};

	public static ITestOutput TestOutput(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string output = DefaultOutput,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new Xunit.Runner.Common.TestOutput()
			{
				AssemblyUniqueID = assemblyUniqueID,
				Output = output,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static ITestPassed TestPassed(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = DefaultExecutionTime,
		DateTimeOffset? finishTime = null,
		string output = DefaultOutput,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID,
		string[]? warnings = null) =>
			new Xunit.Runner.Common.TestPassed()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = executionTime,
				FinishTime = finishTime ?? DefaultFinishTime,
				Output = output,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
				Warnings = warnings,
			};

	public static ITestSkipped TestSkipped(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		DateTimeOffset? finishTime = null,
		string output = DefaultOutput,
		string reason = DefaultSkipReason,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID,
		string[]? warnings = null) =>
			new Xunit.Runner.Common.TestSkipped()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = 0m,
				FinishTime = finishTime ?? DefaultFinishTime,
				Output = output,
				Reason = reason,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
				Warnings = warnings,
			};

	public static ITestStarting TestStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		bool @explicit = false,
		DateTimeOffset? startTime = null,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string testDisplayName = DefaultTestDisplayName,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID,
		int timeout = 0,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null) =>
			new Xunit.Runner.Common.TestStarting()
			{
				AssemblyUniqueID = assemblyUniqueID,
				Explicit = @explicit,
				StartTime = startTime ?? DefaultStartTime,
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
