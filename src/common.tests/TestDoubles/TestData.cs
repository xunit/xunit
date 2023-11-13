using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public static class TestData
{
	public const string DefaultAssemblyName = "test-assembly";
	public const string DefaultAssemblyPath = "./test-assembly.dll";
	public const string DefaultAssemblyUniqueID = "assembly-id";
	public const string DefaultConfigFilePath = "./test-assembly.json";
	public const int DefaultCountFailed = 42;
	public const int DefaultCountTotal = 2112;
	public const int DefaultCountSkipped = 6;
	public const int DefaultCountNotRun = 3;
	public static int[] DefaultExceptionParentIndices = new[] { -1 };
	public static string[] DefaultExceptionMessages = new[] { "Attempted to divide by zero. Did you really think that was going to work?" };
	public static string?[] DefaultExceptionTypes = new[] { typeof(DivideByZeroException).FullName };
	public const decimal DefaultExecutionTime = 123.4567m;
	public const string DefaultSkipReason = "skip-reason";
	public static string?[] DefaultStackTraces = new[] { $"/path/file.cs(42,0): at SomeInnerCall(){Environment.NewLine}/path/otherFile.cs(2112,0): at SomeOuterMethod" };
	public const string DefaultTargetFramework = ".NETMagic,Version=v98.76.54";
	public const string DefaultTestCaseDisplayName = "test-case-display-name";
	public const string DefaultTestCaseUniqueID = "test-case-id";
	public const string DefaultTestCaseSerialization = "test-case-serialization";
	public const string DefaultTestClassName = "test-class-name";
	public const string DefaultTestClassNamespace = "test-class-namespace";
	public const string DefaultTestClassUniqueID = "test-class-id";
	public const string DefaultTestCollectionClass = "test-collection-class";
	public const string DefaultTestCollectionDisplayName = "test-collection-display-name";
	public const string DefaultTestCollectionUniqueID = "test-collection-id";
	public const string DefaultTestDisplayName = "test-display-name";
	public const string DefaultTestEnvironment = "test-environment";
	public const string DefaultTestFrameworkDisplayName = "test-framework";
	public const string DefaultTestMethodName = "test-method";
	public const string DefaultTestMethodUniqueID = "test-method-id";
	public const string DefaultTestUniqueID = "test-id";

	public static _IReflectionAssemblyInfo AssemblyInfo(Assembly assembly) =>
		Reflector.Wrap(assembly);

	public static _DiscoveryComplete DiscoveryComplete(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		int testCasesToRun = DefaultCountTotal) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCasesToRun = testCasesToRun,
			};

	public static _DiscoveryStarting DiscoveryStarting(
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

	public static _ErrorMessage ErrorMessage(
		int[] exceptionParentIndices,
		string?[] exceptionTypes,
		string[] messages,
		string?[] stackTraces) =>
			new()
			{
				ExceptionParentIndices = exceptionParentIndices,
				ExceptionTypes = exceptionTypes,
				Messages = messages,
				StackTraces = stackTraces,
			};

	static IReadOnlyDictionary<string, IReadOnlyList<string>> GetTraits(MethodInfo method)
	{
		var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		foreach (var traitAttribute in method.GetCustomAttributesData().Where(cad => cad.AttributeType.IsAssignableFrom(typeof(TraitAttribute))))
		{
			var ctorArgs = traitAttribute.ConstructorArguments.ToList();
			result.Add((string)ctorArgs[0].Value!, (string)ctorArgs[1].Value!);
		}

		return result.ToReadOnly();
	}

	public static _IReflectionMethodInfo MethodInfo<TClass>(string methodName) =>
		Guard.ArgumentNotNull(
			$"Could not find method '{methodName}' on '{typeof(TClass).FullName}'",
			Reflector.Wrap(typeof(TClass).GetMethod(methodName)),
			nameof(methodName)
		);

	public static TestAssembly TestAssembly(
		Assembly assembly,
		string? configFileName = null,
		Version? version = null) =>
			new(Reflector.Wrap(assembly), configFileName, version);

	public static TestAssembly TestAssembly<TClassUnderTest>(
		string? configFileName = null,
		Version? version = null) =>
			new(Reflector.Wrap(typeof(TClassUnderTest).Assembly), configFileName, version);

	public static _TestAssemblyFinished TestAssemblyFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = DefaultExecutionTime,
		int testsFailed = DefaultCountFailed,
		int testsNotRun = DefaultCountNotRun,
		int testsSkipped = DefaultCountSkipped,
		int testsTotal = DefaultCountTotal) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = executionTime,
				TestsFailed = testsFailed,
				TestsNotRun = testsNotRun,
				TestsSkipped = testsSkipped,
				TestsTotal = testsTotal,
			};

	public static _TestAssemblyStarting TestAssemblyStarting(
		string assemblyName = DefaultAssemblyName,
		string? assemblyPath = DefaultAssemblyPath,
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string? configFilePath = DefaultConfigFilePath,
		int? seed = null,
		DateTimeOffset? startTime = null,
		string? targetFramework = DefaultTargetFramework,
		string testEnvironment = DefaultTestEnvironment,
		string testFrameworkDisplayName = DefaultTestFrameworkDisplayName) =>
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
			};

	public static _TestAssemblyCleanupFailure TestAssemblyCleanupFailure(
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

	public static TestAssemblyDiscoveryFinished TestAssemblyDiscoveryFinished(
		string assemblyPath = DefaultAssemblyPath,
		string configFilePath = DefaultConfigFilePath,
		string targetFramework = DefaultTargetFramework,
		bool diagnosticMessages = false,
		bool internalDiagnosticMessages = false,
		TestMethodDisplay methodDisplay = TestMethodDisplay.ClassAndMethod,
		TestMethodDisplayOptions methodDisplayOptions = TestMethodDisplayOptions.None,
		bool preEnumerateTheories = false,
		int testCasesToRun = 42)
	{
		var project = new XunitProject();
		var assembly = new XunitProjectAssembly(project)
		{
			AssemblyFileName = assemblyPath,
			ConfigFileName = configFilePath,
			TargetFramework = targetFramework
		};
		// See the ForDiscovery method to see which TestAssemblyConfiguration options are used for discovery
		var discoveryOptions = _TestFrameworkOptions.ForDiscovery(new TestAssemblyConfiguration
		{
			DiagnosticMessages = diagnosticMessages,
			InternalDiagnosticMessages = internalDiagnosticMessages,
			MethodDisplay = methodDisplay,
			MethodDisplayOptions = methodDisplayOptions,
			PreEnumerateTheories = preEnumerateTheories
		});

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
		string targetFramework = DefaultTargetFramework,
		bool diagnosticMessages = false,
		bool internalDiagnosticMessages = false,
		TestMethodDisplay methodDisplay = TestMethodDisplay.ClassAndMethod,
		TestMethodDisplayOptions methodDisplayOptions = TestMethodDisplayOptions.None,
		bool preEnumerateTheories = false,
		bool shadowCopy = false)
	{
		var project = new XunitProject();
		var assembly = new XunitProjectAssembly(project)
		{
			AssemblyFileName = assemblyPath,
			ConfigFileName = configFilePath,
			TargetFramework = targetFramework
		};
		// See the ForDiscovery method to see which TestAssemblyConfiguration options are used for discovery
		var discoveryOptions = _TestFrameworkOptions.ForDiscovery(new TestAssemblyConfiguration
		{
			DiagnosticMessages = diagnosticMessages,
			InternalDiagnosticMessages = internalDiagnosticMessages,
			MethodDisplay = methodDisplay,
			MethodDisplayOptions = methodDisplayOptions,
			PreEnumerateTheories = preEnumerateTheories
		});

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
		string targetFramework = DefaultTargetFramework,
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
		var assembly = new XunitProjectAssembly(project)
		{
			AssemblyFileName = assemblyPath,
			ConfigFileName = configFilePath,
			TargetFramework = targetFramework
		};
		// See the ForExecution method to see which TestAssemblyConfiguration options are used for discovery
		var executionOptions = _TestFrameworkOptions.ForExecution(new TestAssemblyConfiguration
		{
			DiagnosticMessages = diagnosticMessages,
			InternalDiagnosticMessages = internalDiagnosticMessages,
			MaxParallelThreads = maxParallelThreads,
			ParallelizeTestCollections = parallelizeTestCollections
		});
		var executionSummary = new ExecutionSummary
		{
			Errors = testsErrored,
			Failed = testsFailed,
			NotRun = testsNotRun,
			Skipped = testsSkipped,
			Time = executionTime,
			Total = testsTotal
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
		string targetFramework = DefaultTargetFramework,
		bool diagnosticMessages = false,
		ExplicitOption? explicitOption = null,
		bool internalDiagnosticMessages = false,
		int maxParallelThreads = 2600,
		bool? parallelizeTestCollections = null,
		bool? stopOnFail = null,
		int? seed = null,
		string? culture = null)
	{
		var project = new XunitProject();
		var assembly = new XunitProjectAssembly(project)
		{
			AssemblyFileName = assemblyPath,
			ConfigFileName = configFilePath,
			TargetFramework = targetFramework
		};
		// See the ForExecution method to see which TestAssemblyConfiguration options are used for discovery
		var executionOptions = _TestFrameworkOptions.ForExecution(new TestAssemblyConfiguration
		{
			Culture = culture,
			DiagnosticMessages = diagnosticMessages,
			ExplicitOption = explicitOption,
			InternalDiagnosticMessages = internalDiagnosticMessages,
			MaxParallelThreads = maxParallelThreads,
			ParallelizeTestCollections = parallelizeTestCollections,
			StopOnFail = stopOnFail,
		});

		return new()
		{
			Assembly = assembly,
			ExecutionOptions = executionOptions,
			Seed = seed,
		};
	}

	public static _TestCaseDiscovered TestCaseDiscovered<TClass>(
		string testMethod,
		string? testCaseDisplayName = null)
	{
		var typeInfo = typeof(TClass);
		var methodInfo = Guard.NotNull($"Could not find method '{testMethod}' in type '{typeInfo.FullName}'", typeInfo.GetMethod(testMethod));
		var skipReason = methodInfo.GetCustomAttribute<FactAttribute>()?.Skip;
		var traits = GetTraits(methodInfo);

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
			typeInfo.Name,
			testClassUniqueID,
			typeInfo.FullName,
			DefaultTestCollectionUniqueID,
			testMethod,
			testMethodUniqueID,
			typeInfo.Namespace,
			traits
		);
	}

	public static _TestCaseCleanupFailure TestCaseCleanupFailure(
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

	public static _TestCaseDiscovered TestCaseDiscovered(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string serialization = DefaultTestCaseSerialization,
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = DefaultTestCaseDisplayName,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClass = null,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string? testClassWithNamespace = null,
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
				TestClassName = testClass,
				TestClassNameWithNamespace = testClassWithNamespace,
				TestClassNamespace = testNamespace,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodName = testMethod,
				TestMethodUniqueID = testMethodUniqueID,
				Traits = traits ?? new Dictionary<string, IReadOnlyList<string>>(),
			};

	public static _TestCaseFinished TestCaseFinished(
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

	public static _TestCaseStarting TestCaseStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = DefaultTestCaseDisplayName,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string testClassName = DefaultTestClassName,
		string testClassNamespace = DefaultTestClassNamespace,
		string? testClassNameWithNamespace = null,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string testMethodName = DefaultTestMethodName,
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
				TestClassNameWithNamespace = testClassNameWithNamespace ?? $"{(string.IsNullOrWhiteSpace(testClassNamespace) ? "" : testClassNamespace + ".")}{testClassName}",
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodName = testMethodName,
				TestMethodUniqueID = testMethodUniqueID,
				Traits = traits ?? new Dictionary<string, IReadOnlyList<string>>(),
			};

	public static _TestClassCleanupFailure TestClassCleanupFailure(
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

	public static _TestClassFinished TestClassFinished(
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

	public static _TestClassStarting TestClassStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string testClass = DefaultTestClassName,
		string testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestClass = testClass,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
			};

	public static _TestCleanupFailure TestCleanupFailure(
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

	public static TestCollection TestCollection(
		_ITestAssembly assembly,
		_ITypeInfo? collectionDefinition = null,
		string? displayName = null) =>
			new(assembly, collectionDefinition, displayName ?? $"[Unit Test] Collection for '{assembly.Assembly.Name}'");

	public static _TestCollectionCleanupFailure TestCollectionCleanupFailure(
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

	public static _TestCollectionFinished TestCollectionFinished(
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

	public static _TestCollectionStarting TestCollectionStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string? testCollectionClass = DefaultTestCollectionClass,
		string testCollectionDisplayName = DefaultTestCollectionDisplayName,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCollectionClass = testCollectionClass,
				TestCollectionDisplayName = testCollectionDisplayName,
				TestCollectionUniqueID = testCollectionUniqueID,
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

	public static _TestFailed TestFailed(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		FailureCause cause = FailureCause.Assertion,
		int[]? exceptionParentIndices = null,
		string?[]? exceptionTypes = null,
		decimal executionTime = DefaultExecutionTime,
		string[]? messages = null,
		string output = "",
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

	public static _TestFinished TestFinished(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = DefaultExecutionTime,
		string output = "",
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

	public static TestMethod TestMethod(
		_ITestClass testClass,
		_IMethodInfo methodInfo,
		string? uniqueID = null) =>
			new(testClass, methodInfo, uniqueID);

	public static TestMethod TestMethod<TClass>(
		string methodName,
		_ITestCollection? collection = null,
		string? uniqueID = null)
	{
		var assembly = Reflector.Wrap(typeof(TClass).Assembly);
		var testAssembly = new TestAssembly(assembly, uniqueID: "assembly-id");
		collection ??= new TestCollection(testAssembly, null, $"Test data: Test Collection for '{typeof(TClass).FullName}'", uniqueID: "collection-id");
		var @class = Reflector.Wrap(typeof(TClass));
		var testClass = new TestClass(collection, @class, uniqueID: "class-id");
		var method = typeof(TClass).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
		Guard.ArgumentNotNull($"Could not find method '{methodName}' on type '{typeof(TClass).FullName}'", method, nameof(methodName));
		var methodInfo = Reflector.Wrap(method);

		return new(testClass, methodInfo, uniqueID ?? "method-id");
	}

	public static _TestMethodCleanupFailure TestMethodCleanupFailure(
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

	public static _TestMethodFinished TestMethodFinished(
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

	public static _TestMethodStarting TestMethodStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string testMethod = DefaultTestMethodName,
		string testMethodUniqueID = DefaultTestMethodUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethod = testMethod,
				TestMethodUniqueID = testMethodUniqueID,
			};

	public static _TestNotRun TestNotRun(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
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
				Output = "",
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
				Warnings = warnings,
			};

	public static _TestPassed TestPassed(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		decimal executionTime = DefaultExecutionTime,
		string output = "",
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

	public static _TestSkipped TestSkipped(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
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
				Output = "",
				Reason = reason,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
				Warnings = warnings,
			};

	public static _TestStarting TestStarting(
		string assemblyUniqueID = DefaultAssemblyUniqueID,
		string testCaseUniqueID = DefaultTestCaseUniqueID,
		string? testClassUniqueID = DefaultTestClassUniqueID,
		string testCollectionUniqueID = DefaultTestCollectionUniqueID,
		string testDisplayName = DefaultTestDisplayName,
		string? testMethodUniqueID = DefaultTestMethodUniqueID,
		string testUniqueID = DefaultTestUniqueID) =>
			new()
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestDisplayName = testDisplayName,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID,
			};

	public static _IReflectionTypeInfo TypeInfo<TClass>() =>
		Reflector.Wrap(typeof(TClass));

	public static XunitDelayEnumeratedTheoryTestCase XunitDelayEnumeratedTheoryTestCase<TClassUnderTest>(
		string methodName,
		_ITestCollection? collection = null,
		TestMethodDisplay methodDisplay = TestMethodDisplay.ClassAndMethod,
		TestMethodDisplayOptions methodDisplayOptions = TestMethodDisplayOptions.None,
		bool @explicit = false,
		Dictionary<string, List<string>>? traits = null,
		int timeout = 0,
		string? uniqueID = null)
	{
		var testMethod = TestMethod<TClassUnderTest>(methodName, collection);
		var theoryAttribute = Mocks.TheoryAttribute(@explicit: @explicit, timeout: timeout);
		var discoveryOptions = _TestFrameworkOptions.ForDiscovery(methodDisplay: methodDisplay, methodDisplayOptions: methodDisplayOptions);
		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, theoryAttribute);
		if (traits is null)
			traits = TestIntrospectionHelper.GetTraits(testMethod);

		return new(
			details.ResolvedTestMethod,
			details.TestCaseDisplayName,
			uniqueID ?? details.UniqueID,
			@explicit,
			traits,
			timeout: timeout
		);
	}

	public static XunitTestCase XunitTestCase<TClassUnderTest>(
		string methodName,
		_ITestCollection? collection = null,
		TestMethodDisplay methodDisplay = TestMethodDisplay.ClassAndMethod,
		TestMethodDisplayOptions methodDisplayOptions = TestMethodDisplayOptions.None,
		string? skipReason = null,
		bool @explicit = false,
		Dictionary<string, List<string>>? traits = null,
		object?[]? testMethodArguments = null,
		int timeout = 0,
		string? uniqueID = null)
	{
		var testMethod = TestMethod<TClassUnderTest>(methodName, collection);
		var factAttribute = testMethod.Method.GetCustomAttributes(typeof(FactAttribute)).Single();
		var discoveryOptions = _TestFrameworkOptions.ForDiscovery(methodDisplay: methodDisplay, methodDisplayOptions: methodDisplayOptions);
		var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);
		if (traits is null)
			traits = TestIntrospectionHelper.GetTraits(testMethod);

		return new(
			details.ResolvedTestMethod,
			details.TestCaseDisplayName,
			uniqueID ?? details.UniqueID,
			@explicit,
			skipReason,
			traits,
			testMethodArguments,
			timeout: timeout
		);
	}
}
