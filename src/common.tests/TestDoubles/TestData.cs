using System;
using System.Collections.Generic;

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

		public static _TestCollectionFinished TestCollectionFinished(
			decimal executionTime = DefaultExecutionTime,
			string testCollectionUniqueID = DefaultTestCollectionUniqueID,
			int testsFailed = DefaultCountFailed,
			int testsRun = DefaultCountRun,
			int testsSkipped = DefaultCountSkipped) =>
				new _TestCollectionFinished
				{
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
	}
}
