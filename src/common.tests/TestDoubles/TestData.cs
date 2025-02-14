using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

#if XUNIT_AOT
using System.IO;
#else
using System.Runtime.Versioning;
#endif

// This file contains test data that doesn't fit it any other categorization
public static partial class TestData
{
	public const string DefaultAttributeName = "attribute-name";
	public const string DefaultAssemblyName = "test-assembly";
	public const string DefaultAssemblyPath = "./test-assembly.dll";
	public const string DefaultAssemblyUniqueID = "assembly-id";
	public const string DefaultConfigFilePath = "./test-assembly.json";
	public const int DefaultCountFailed = 42;
	public const int DefaultCountTotal = 2112;
	public const int DefaultCountSkipped = 6;
	public const int DefaultCountNotRun = 3;
	public static readonly int[] DefaultExceptionParentIndices = [-1];
	public static readonly string[] DefaultExceptionMessages = ["Attempted to divide by zero. Did you really think that was going to work?"];
	public static readonly string?[] DefaultExceptionTypes = [typeof(DivideByZeroException).FullName];
	public const decimal DefaultExecutionTime = 123.4567m;
	public static DateTimeOffset DefaultFinishTime = new(2024, 07, 04, 21, 12, 9, TimeSpan.Zero);
	public const string DefaultMethodName = "test-method";
	public static string[] DefaultMethodParameterTypes = [typeof(int).ToVSTestTypeName(), typeof(string).ToVSTestTypeName()];
	public static string DefaultMethodReturnType = typeof(void).ToVSTestTypeName();
	public static Guid DefaultModuleVersionID = new("61aa43e6-1985-4c43-95c4-b146498925a2");
	public const string DefaultOutput = "test-helper-output";
	public const string DefaultSkipReason = "skip-reason";
	public static readonly string?[] DefaultStackTraces = [$"/path/file.cs(42,0): at SomeInnerCall(){Environment.NewLine}/path/otherFile.cs(2112,0): at SomeOuterMethod"];
	public static DateTimeOffset DefaultStartTime = new(2024, 07, 04, 21, 12, 8, TimeSpan.Zero);
	public const string DefaultTargetFramework = ".NETMagic,Version=v98.76.54";
	public const string DefaultTestCaseDisplayName = "test-case-display-name";
	public const string DefaultTestCaseUniqueID = "test-case-id";
	public const string DefaultTestCaseSerialization = "test-case-serialization";
	public const int DefaultTestClassMetadataToken = 2600;
	public const string DefaultTestClassName = "test-class-name";
	public const string DefaultTestClassNamespace = "test-class-namespace";
	public const string DefaultTestClassSimpleName = "test-class-simple-name";
	public const string DefaultTestClassUniqueID = "test-class-id";
	public const string DefaultTestCollectionClass = "test-collection-class";
	public const string DefaultTestCollectionDisplayName = "test-collection-display-name";
	public const string DefaultTestCollectionUniqueID = "test-collection-id";
	public const string DefaultTestDisplayName = "test-display-name";
	public const string DefaultTestEnvironment = "test-environment";
	public const string DefaultTestFrameworkDisplayName = "test-framework";
	public const int DefaultTestMethodMetadataToken = 5200;
	public const string DefaultTestMethodUniqueID = "test-method-id";
	public const string DefaultTestUniqueID = "test-id";
	public static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> DefaultTraits = new Dictionary<string, HashSet<string>>() { ["foo"] = ["bar", "baz"], ["biff"] = ["bang"] }.ToReadOnly();
	public static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> DefaultTraitsWithCategory = new Dictionary<string, HashSet<string>>() { ["foo"] = ["bar", "baz"], ["biff"] = ["bang"], ["category"] = ["interesting"] }.ToReadOnly();
	public static readonly IReadOnlyDictionary<string, TestAttachment> EmptyAttachments = new Dictionary<string, TestAttachment>();
	public static readonly Dictionary<string, (Type, CollectionDefinitionAttribute)> EmptyCollectionDefinitions = [];
	public static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> EmptyTraits = new Dictionary<string, IReadOnlyCollection<string>>();

	public static ITestFrameworkDiscoveryOptions TestFrameworkDiscoveryOptions(
		string? culture = null,
		bool? diagnosticMessages = null,
		bool? includeSourceInformation = null,
		bool? internalDiagnosticMessages = null,
		TestMethodDisplay? methodDisplay = null,
		TestMethodDisplayOptions? methodDisplayOptions = null,
		bool? preEnumerateTheories = null,
		int? printMaxEnumerableLength = null,
		int? printMaxObjectDepth = null,
		int? printMaxObjectMemberCount = null,
		int? printMaxStringLength = null,
		bool? synchronousMessageReporting = null)
	{
		ITestFrameworkDiscoveryOptions result = TestFrameworkOptions.Empty();

		result.SetCulture(culture);
		result.SetDiagnosticMessages(diagnosticMessages);
		result.SetIncludeSourceInformation(includeSourceInformation);
		result.SetInternalDiagnosticMessages(internalDiagnosticMessages);
		result.SetMethodDisplay(methodDisplay);
		result.SetMethodDisplayOptions(methodDisplayOptions);
		result.SetPreEnumerateTheories(preEnumerateTheories);
		result.SetPrintMaxEnumerableLength(printMaxEnumerableLength);
		result.SetPrintMaxObjectDepth(printMaxObjectDepth);
		result.SetPrintMaxObjectMemberCount(printMaxObjectMemberCount);
		result.SetPrintMaxStringLength(printMaxStringLength);
		result.SetSynchronousMessageReporting(synchronousMessageReporting);

		return result;
	}

	public static ITestFrameworkExecutionOptions TestFrameworkExecutionOptions(
		int? assertEquivalentMaxDepth = null,
		string? culture = null,
		bool? diagnosticMessages = null,
		bool? disableParallelization = null,
		ExplicitOption? explicitOption = null,
		bool? failSkips = null,
		bool? failTestsWithWarnings = null,
		bool? internalDiagnosticMessages = null,
		int? maxParallelThreads = null,
		ParallelAlgorithm? parallelAlgorithm = null,
		int? printMaxEnumerableLength = null,
		int? printMaxObjectDepth = null,
		int? printMaxObjectMemberCount = null,
		int? printMaxStringLength = null,
		int? seed = null,
		bool? showLiveOutput = null,
		bool? stopOnFail = null,
		bool? synchronousMessageReporting = null)
	{
		ITestFrameworkExecutionOptions result = TestFrameworkOptions.Empty();

		result.SetAssertEquivalentMaxDepth(assertEquivalentMaxDepth);
		result.SetCulture(culture);
		result.SetDiagnosticMessages(diagnosticMessages);
		result.SetDisableParallelization(disableParallelization);
		result.SetExplicitOption(explicitOption);
		result.SetFailSkips(failSkips);
		result.SetFailTestsWithWarnings(failTestsWithWarnings);
		result.SetInternalDiagnosticMessages(internalDiagnosticMessages);
		result.SetMaxParallelThreads(maxParallelThreads);
		result.SetParallelAlgorithm(parallelAlgorithm);
		result.SetPrintMaxEnumerableLength(printMaxEnumerableLength);
		result.SetPrintMaxObjectDepth(printMaxObjectDepth);
		result.SetPrintMaxObjectMemberCount(printMaxObjectMemberCount);
		result.SetPrintMaxStringLength(printMaxStringLength);
		result.SetSeed(seed);
		result.SetShowLiveOutput(showLiveOutput);
		result.SetStopOnTestFail(stopOnFail);
		result.SetSynchronousMessageReporting(synchronousMessageReporting);

		return result;
	}

	public static XunitProjectAssembly XunitProjectAssembly<TTestClass>(
		XunitProject? project = null,
		int xUnitVersion = 3)
	{
#if XUNIT_AOT
		var assemblySimpleName = Guard.NotNull("Assembly.GetEntryAssembly().GetName().Name returned null", Assembly.GetEntryAssembly()?.GetName().Name) + ".dll";
		var assemblyFileName = Path.Combine(AppContext.BaseDirectory, assemblySimpleName);
#if NET8_0
		var targetFramework = ".NETCoreApp,Version=v8.0";
#elif NET9_0
		var targetFramework = ".NETCoreApp,Version=v9.0";
#else
#error Unknown target framework
#endif
#else
		var assemblyFileName = typeof(TTestClass).Assembly.Location;
		var targetFramework =
			typeof(TTestClass).Assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName
				?? throw new InvalidOperationException($"Assembly '{assemblyFileName}' does not have an assembly-level TargetFrameworkAttribute");
#endif

		var assemblyMetadata = new AssemblyMetadata(xUnitVersion, targetFramework);
		return new(project ?? new XunitProject(), assemblyFileName, assemblyMetadata);
	}
}
