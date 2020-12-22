using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Runner.v2;
using Xunit.Sdk;

namespace Xunit.v3
{
	public static class TestData
	{
		public const string DefaultAssemblyName = "test-assembly";
		public const string DefaultAssemblyPath = "./test-assembly.dll";
		public const string DefaultAssemblyUniqueID = "assembly-id";
		public const string DefaultConfigFilePath = "./test-assembly.json";
		public const int DefaultCountFailed = 42;
		public const int DefaultCountRun = 2112;
		public const int DefaultCountSkipped = 6;
		public static int[] DefaultExceptionParentIndices = new[] { -1 };
		public static string?[] DefaultExceptionTypes = new[] { typeof(DivideByZeroException).FullName };
		public static string[] DefaultExceptionMessages = new[] { "Attempted to divide by zero. Did you really think that was going to work?" };
		public static string?[] DefaultStackTraces = new[] { $"/path/file.cs(42,0): at SomeInnerCall(){Environment.NewLine}/path/otherFile.cs(2112,0): at SomeOuterMethod" };
		public const decimal DefaultExecutionTime = 123.4567m;
		public const string DefaultTestCaseUniqueID = "test-case-id";
		public const string DefaultTestClassUniqueID = "test-class-id";
		public const string DefaultTestCollectionUniqueID = "test-collection-id";
		public const string DefaultTestMethodUniqueID = "test-method-id";
		public const string DefaultTestUniqueID = "test-id";

		public static _ErrorMessage ErrorMessage(
			int[] exceptionParentIndices,
			string?[] exceptionTypes,
			string[] messages,
			string?[] stackTraces) =>
				new _ErrorMessage
				{
					ExceptionParentIndices = exceptionParentIndices,
					ExceptionTypes = exceptionTypes,
					Messages = messages,
					StackTraces = stackTraces
				};

		public static TestAssembly TestAssembly(
			Assembly assembly,
			string? configFileName = null,
			Version? version = null) =>
				new TestAssembly(Reflector.Wrap(assembly), configFileName, version);

		public static TestAssembly TestAssembly<TClassUnderTest>(
			string? configFileName = null,
			Version? version = null) =>
				new TestAssembly(Reflector.Wrap(typeof(TClassUnderTest).Assembly), configFileName, version);

		public static _TestAssemblyFinished TestAssemblyFinished(
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			decimal executionTime = DefaultExecutionTime,
			int testsFailed = DefaultCountFailed,
			int testsRun = DefaultCountRun,
			int testsSkipped = DefaultCountSkipped) =>
				new _TestAssemblyFinished
				{
					AssemblyUniqueID = assemblyUniqueID,
					ExecutionTime = executionTime,
					TestsFailed = testsFailed,
					TestsRun = testsRun,
					TestsSkipped = testsSkipped
				};

		public static _TestAssemblyStarting TestAssemblyStarting(
			string assemblyName = DefaultAssemblyName,
			string? assemblyPath = DefaultAssemblyPath,
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			string? configFilePath = DefaultConfigFilePath,
			DateTimeOffset? startTime = null,
			string targetFramework = ".NETMagic,Version=v98.76.54",
			string testEnvironment = "test-environment",
			string testFrameworkDisplayName = "test-framework") =>
				new _TestAssemblyStarting
				{
					AssemblyName = assemblyName,
					AssemblyPath = assemblyPath,
					AssemblyUniqueID = assemblyUniqueID,
					ConfigFilePath = configFilePath,
					StartTime = startTime ?? new DateTimeOffset(2021, 1, 20, 17, 0, 0, TimeSpan.Zero),
					TargetFramework = targetFramework,
					TestEnvironment = testEnvironment,
					TestFrameworkDisplayName = testFrameworkDisplayName
				};

		public static _TestAssemblyCleanupFailure TestAssemblyCleanupFailure(
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			int[]? exceptionParentIndices = null,
			string?[]? exceptionTypes = null,
			string[]? messages = null,
			string?[]? stackTraces = null) =>
				new _TestAssemblyCleanupFailure
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
			bool diagnosticMessages = false,
			bool internalDiagnosticMessages = false,
			TestMethodDisplay methodDisplay = TestMethodDisplay.ClassAndMethod,
			TestMethodDisplayOptions methodDisplayOptions = TestMethodDisplayOptions.None,
			bool preEnumerateTheories = false,
			int testCasesDiscovered = 2112,
			int testCasesToRun = 42)
		{
			var assembly = new XunitProjectAssembly
			{
				AssemblyFilename = assemblyPath,
				ConfigFilename = configFilePath
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

			return new TestAssemblyDiscoveryFinished
			{
				Assembly = assembly,
				DiscoveryOptions = discoveryOptions,
				TestCasesDiscovered = testCasesDiscovered,
				TestCasesToRun = testCasesToRun
			};
		}

		public static TestAssemblyDiscoveryStarting TestAssemblyDiscoveryStarting(
			AppDomainOption appDomain = AppDomainOption.Disabled,
			string assemblyPath = DefaultAssemblyPath,
			string configFilePath = DefaultConfigFilePath,
			bool diagnosticMessages = false,
			bool internalDiagnosticMessages = false,
			TestMethodDisplay methodDisplay = TestMethodDisplay.ClassAndMethod,
			TestMethodDisplayOptions methodDisplayOptions = TestMethodDisplayOptions.None,
			bool preEnumerateTheories = false,
			bool shadowCopy = false)
		{
			var assembly = new XunitProjectAssembly
			{
				AssemblyFilename = assemblyPath,
				ConfigFilename = configFilePath
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

			return new TestAssemblyDiscoveryStarting
			{
				AppDomain = appDomain,
				Assembly = assembly,
				DiscoveryOptions = discoveryOptions,
				ShadowCopy = shadowCopy
			};
		}

		public static TestAssemblyExecutionFinished TestAssemblyExecutionFinished(
			string assemblyPath = DefaultAssemblyPath,
			string configFilePath = DefaultConfigFilePath,
			bool diagnosticMessages = false,
			decimal executionTime = 123.4567m,
			bool internalDiagnosticMessages = false,
			int maxParallelThreads = 2600,
			bool parallelizeTestCollections = false,
			int testsErrored = 95,
			int testsFailed = DefaultCountFailed,
			int testsRun = DefaultCountRun,
			int testsSkipped = DefaultCountSkipped)
		{
			var assembly = new XunitProjectAssembly
			{
				AssemblyFilename = assemblyPath,
				ConfigFilename = configFilePath
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
				Skipped = testsSkipped,
				Time = executionTime,
				Total = testsRun
			};

			return new TestAssemblyExecutionFinished
			{
				Assembly = assembly,
				ExecutionOptions = executionOptions,
				ExecutionSummary = executionSummary
			};
		}

		public static TestAssemblyExecutionStarting TestAssemblyExecutionStarting(
			string assemblyPath = DefaultAssemblyPath,
			string configFilePath = DefaultConfigFilePath,
			bool diagnosticMessages = false,
			bool internalDiagnosticMessages = false,
			int maxParallelThreads = 2600,
			bool parallelizeTestCollections = false)
		{
			var assembly = new XunitProjectAssembly
			{
				AssemblyFilename = assemblyPath,
				ConfigFilename = configFilePath
			};
			// See the ForExecution method to see which TestAssemblyConfiguration options are used for discovery
			var executionOptions = _TestFrameworkOptions.ForExecution(new TestAssemblyConfiguration
			{
				DiagnosticMessages = diagnosticMessages,
				InternalDiagnosticMessages = internalDiagnosticMessages,
				MaxParallelThreads = maxParallelThreads,
				ParallelizeTestCollections = parallelizeTestCollections
			});

			return new TestAssemblyExecutionStarting
			{
				Assembly = assembly,
				ExecutionOptions = executionOptions
			};
		}

		public static _TestCaseFinished TestCaseFinished(
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			decimal executionTime = 123.4567m,
			string testCaseUniqueID = DefaultTestCaseUniqueID,
			string? testClassUniqueID = DefaultTestClassUniqueID,
			string testCollectionUniqueID = DefaultTestCollectionUniqueID,
			string? testMethodUniqueID = DefaultTestMethodUniqueID,
			int testsFailed = DefaultCountFailed,
			int testsRun = DefaultCountRun,
			int testsSkipped = DefaultCountSkipped) =>
				new _TestCaseFinished
				{
					AssemblyUniqueID = assemblyUniqueID,
					ExecutionTime = executionTime,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestsFailed = testsFailed,
					TestsRun = testsRun,
					TestsSkipped = testsSkipped
				};

		public static _TestCaseStarting TestCaseStarting(
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			string? skipReason = null,
			string? sourceFilePath = null,
			int? sourceLineNumber = null,
			string testCaseDisplayName = "test-case-display-name",
			string testCaseUniqueID = DefaultTestCaseUniqueID,
			string? testClassUniqueID = DefaultTestClassUniqueID,
			string testCollectionUniqueID = DefaultTestCollectionUniqueID,
			string? testMethodUniqueID = DefaultTestMethodUniqueID,
			Dictionary<string, List<string>>? traits = null) =>
				new _TestCaseStarting
				{
					AssemblyUniqueID = assemblyUniqueID,
					SkipReason = skipReason,
					SourceFilePath = sourceFilePath,
					SourceLineNumber = sourceLineNumber,
					TestCaseDisplayName = testCaseDisplayName,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					Traits = traits ?? new Dictionary<string, List<string>>()
				};

		public static TestClass TestClass<TClassUnderTest>(_ITestCollection collection) =>
			new TestClass(collection, Reflector.Wrap(typeof(TClassUnderTest)));

		public static TestClass TestClass(
			_ITestCollection collection,
			Type classType) =>
				new TestClass(collection, Reflector.Wrap(classType));

		public static _TestClassFinished TestClassFinished(
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			decimal executionTime = 123.4567m,
			string? testClassUniqueID = DefaultTestClassUniqueID,
			string testCollectionUniqueID = DefaultTestCollectionUniqueID,
			int testsFailed = DefaultCountFailed,
			int testsRun = DefaultCountRun,
			int testsSkipped = DefaultCountSkipped) =>
				new _TestClassFinished
				{
					AssemblyUniqueID = assemblyUniqueID,
					ExecutionTime = executionTime,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestsFailed = testsFailed,
					TestsRun = testsRun,
					TestsSkipped = testsSkipped
				};

		public static _TestClassStarting TestClassStarting(
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			string testClass = "test-class",
			string testClassUniqueID = DefaultTestClassUniqueID,
			string testCollectionUniqueID = DefaultTestCollectionUniqueID) =>
				new _TestClassStarting
				{
					AssemblyUniqueID = assemblyUniqueID,
					TestClass = testClass,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID
				};

		public static TestCollection TestCollection(
			_ITestAssembly assembly,
			ITypeInfo? collectionDefinition = null,
			string? displayName = null) =>
				new TestCollection(assembly, collectionDefinition, displayName ?? $"[Unit Test] Collection for '{assembly.Assembly.Name}'");

		public static _TestCollectionFinished TestCollectionFinished(
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			decimal executionTime = DefaultExecutionTime,
			string testCollectionUniqueID = DefaultTestCollectionUniqueID,
			int testsFailed = DefaultCountFailed,
			int testsRun = DefaultCountRun,
			int testsSkipped = DefaultCountSkipped) =>
				new _TestCollectionFinished
				{
					AssemblyUniqueID = assemblyUniqueID,
					ExecutionTime = executionTime,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestsFailed = testsFailed,
					TestsRun = testsRun,
					TestsSkipped = testsSkipped
				};

		public static _TestCollectionStarting TestCollectionStarting(
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			string? testCollectionClass = "test-collection-class",
			string testCollectionDisplayName = "test-collection-display-name",
			string testCollectionUniqueID = DefaultTestCollectionUniqueID) =>
				new _TestCollectionStarting
				{
					AssemblyUniqueID = assemblyUniqueID,
					TestCollectionClass = testCollectionClass,
					TestCollectionDisplayName = testCollectionDisplayName,
					TestCollectionUniqueID = testCollectionUniqueID
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
			int[]? exceptionParentIndices = null,
			string?[]? exceptionTypes = null,
			decimal executionTime = 123.4567m,
			string[]? messages = null,
			string output = "",
			string?[]? stackTraces = null,
			string testCaseUniqueID = DefaultTestCaseUniqueID,
			string? testClassUniqueID = DefaultTestClassUniqueID,
			string testCollectionUniqueID = DefaultTestCollectionUniqueID,
			string? testMethodUniqueID = DefaultTestMethodUniqueID,
			string testUniqueID = DefaultTestUniqueID) =>
				new _TestFailed
				{
					AssemblyUniqueID = assemblyUniqueID,
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
					TestUniqueID = testUniqueID
				};

		public static _TestFinished TestFinished(
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			decimal executionTime = 123.4567m,
			string output = "",
			string testCaseUniqueID = DefaultTestCaseUniqueID,
			string? testClassUniqueID = DefaultTestClassUniqueID,
			string testCollectionUniqueID = DefaultTestCollectionUniqueID,
			string? testMethodUniqueID = DefaultTestMethodUniqueID,
			string testUniqueID = DefaultTestUniqueID) =>
				new _TestFinished
				{
					AssemblyUniqueID = assemblyUniqueID,
					ExecutionTime = executionTime,
					Output = output,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestUniqueID = testUniqueID
				};

		public static TestMethod TestMethod(
			_ITestClass testClass,
			IMethodInfo methodInfo,
			string? uniqueID = null) =>
				new TestMethod(testClass, methodInfo, uniqueID);

		public static TestMethod TestMethod<TClass>(
			string methodName,
			_ITestCollection? collection = null)
		{
			var assembly = Reflector.Wrap(typeof(TClass).Assembly);
			var testAssembly = new TestAssembly(assembly, uniqueID: "assembly-id");
			collection ??= new TestCollection(testAssembly, null, $"Test data: Test Collection for '{typeof(TClass).FullName}'", uniqueID: "collection-id");
			var @class = Reflector.Wrap(typeof(TClass));
			var testClass = new TestClass(collection, @class, uniqueID: "class-id");
			var method = typeof(TClass).GetMethod(methodName);
			Guard.ArgumentValidNotNull(nameof(methodName), $"Could not find method '{methodName}' on type '{typeof(TClass).FullName}'", method);
			var methodInfo = Reflector.Wrap(method);

			return new TestMethod(testClass, methodInfo, uniqueID: "method-id");
		}

		public static _TestMethodFinished TestMethodFinished(
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			decimal executionTime = 123.4567m,
			string? testClassUniqueID = DefaultTestClassUniqueID,
			string testCollectionUniqueID = DefaultTestCollectionUniqueID,
			string? testMethodUniqueID = DefaultTestMethodUniqueID,
			int testsFailed = DefaultCountFailed,
			int testsRun = DefaultCountRun,
			int testsSkipped = DefaultCountSkipped) =>
				new _TestMethodFinished
				{
					AssemblyUniqueID = assemblyUniqueID,
					ExecutionTime = executionTime,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestsFailed = testsFailed,
					TestsRun = testsRun,
					TestsSkipped = testsSkipped
				};

		public static _TestMethodStarting TestMethodStarting(
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			string testClassUniqueID = DefaultTestClassUniqueID,
			string testCollectionUniqueID = DefaultTestCollectionUniqueID,
			string testMethod = "test-method",
			string testMethodUniqueID = DefaultTestMethodUniqueID) =>
				new _TestMethodStarting
				{
					AssemblyUniqueID = assemblyUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethod = testMethod,
					TestMethodUniqueID = testMethodUniqueID
				};

		public static _TestPassed TestPassed(
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			decimal executionTime = 123.4567m,
			string output = "",
			string testCaseUniqueID = DefaultTestCaseUniqueID,
			string? testClassUniqueID = DefaultTestClassUniqueID,
			string testCollectionUniqueID = DefaultTestCollectionUniqueID,
			string? testMethodUniqueID = DefaultTestMethodUniqueID,
			string testUniqueID = DefaultTestUniqueID) =>
				new _TestPassed
				{
					AssemblyUniqueID = assemblyUniqueID,
					ExecutionTime = executionTime,
					Output = output,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestUniqueID = testUniqueID
				};

		public static _TestSkipped TestSkipped(
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			decimal executionTime = 0m,
			string output = "",
			string reason = "skip-reason",
			string testCaseUniqueID = DefaultTestCaseUniqueID,
			string? testClassUniqueID = DefaultTestClassUniqueID,
			string testCollectionUniqueID = DefaultTestCollectionUniqueID,
			string? testMethodUniqueID = DefaultTestMethodUniqueID,
			string testUniqueID = DefaultTestUniqueID) =>
				new _TestSkipped
				{
					AssemblyUniqueID = assemblyUniqueID,
					ExecutionTime = executionTime,
					Output = output,
					Reason = reason,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestUniqueID = testUniqueID
				};

		public static _TestStarting TestStarting(
			string assemblyUniqueID = DefaultAssemblyUniqueID,
			string testCaseUniqueID = DefaultTestCaseUniqueID,
			string? testClassUniqueID = DefaultTestClassUniqueID,
			string testCollectionUniqueID = DefaultTestCollectionUniqueID,
			string testDisplayName = "test-display-name",
			string? testMethodUniqueID = DefaultTestMethodUniqueID,
			string testUniqueID = DefaultTestUniqueID) =>
				new _TestStarting
				{
					AssemblyUniqueID = assemblyUniqueID,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestDisplayName = testDisplayName,
					TestMethodUniqueID = testMethodUniqueID,
					TestUniqueID = testUniqueID
				};

		public static XunitTestCase XunitTestCase<TClassUnderTest>(
			string methodName,
			_ITestCollection? collection = null,
			object[]? testMethodArguments = null,
			TestMethodDisplay methodDisplay = TestMethodDisplay.ClassAndMethod,
			TestMethodDisplayOptions methodDisplayOptions = TestMethodDisplayOptions.None,
			string? uniqueID = null,
			_IMessageSink? diagnosticMessageSink = null)
		{
			diagnosticMessageSink ??= new _NullMessageSink();

			var method = TestMethod<TClassUnderTest>(methodName, collection);

			return new XunitTestCase(diagnosticMessageSink, methodDisplay, methodDisplayOptions, method, testMethodArguments, uniqueID);
		}

		public static XunitTheoryTestCase XunitTheoryTestCase<TClassUnderTest>(
			string methodName,
			_ITestCollection? collection = null,
			TestMethodDisplay methodDisplay = TestMethodDisplay.ClassAndMethod,
			TestMethodDisplayOptions methodDisplayOptions = TestMethodDisplayOptions.None,
			string? uniqueID = null,
			_IMessageSink? diagnosticMessageSink = null)
		{
			diagnosticMessageSink ??= new _NullMessageSink();

			var method = TestMethod<TClassUnderTest>(methodName, collection);

			return new XunitTheoryTestCase(diagnosticMessageSink, methodDisplay, methodDisplayOptions, method, uniqueID);
		}
	}
}
