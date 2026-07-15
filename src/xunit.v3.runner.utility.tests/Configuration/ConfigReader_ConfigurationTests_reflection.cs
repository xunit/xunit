#if NETFRAMEWORK

using System.Reflection;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public static class ConfigReader_ConfigurationTests
{
	static readonly string AssemblyFileName;
	static readonly string AssemblyPath;

	static ConfigReader_ConfigurationTests()
	{
		AssemblyFileName = Assembly.GetExecutingAssembly().GetLocalCodeBase();
		AssemblyPath = Path.GetDirectoryName(AssemblyFileName)!;
	}

	[Fact]
	public static void ConfigurationFileNotFound_ReturnsFalseWithWarning()
	{
		var configuration = new TestAssemblyConfiguration();
		var configFilePath = Path.Combine(AssemblyPath, "UnknownFile.config");
		var warnings = new List<string>();

		var result = ConfigReader_Configuration.Load(configuration, AssemblyFileName, configFilePath, warnings);

		Assert.False(result);
		var warning = Assert.Single(warnings);
		Assert.Equal($"Couldn't load config file '{configFilePath}': file not found", warning);
	}

	[Fact]
	public static void EmptyConfigurationFile_ReturnsDefaultValues()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Configuration.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_Empty.config"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(AppDomainSupport.IfAvailable, configuration.AppDomainOrDefault);
		Assert.Null(configuration.AssertEquivalentMaxDepth);
		Assert.Null(configuration.Culture);
		Assert.False(configuration.DiagnosticMessagesOrDefault);
		Assert.False(configuration.FailSkipsOrDefault);
		Assert.False(configuration.InternalDiagnosticMessagesOrDefault);
		Assert.Equal(-1, configuration.LongRunningTestSecondsOrDefault);
		Assert.Equal(Environment.ProcessorCount, configuration.MaxParallelThreadsOrDefault);
		Assert.Equal(TestMethodDisplay.ClassAndMethod, configuration.MethodDisplayOrDefault);
		Assert.Equal(TestMethodDisplayOptions.None, configuration.MethodDisplayOptionsOrDefault);
		Assert.Equal(ParallelAlgorithm.Conservative, configuration.ParallelAlgorithmOrDefault);
		Assert.False(configuration.ParallelizeAssemblyOrDefault);
		Assert.Equal(ParallelMode.Collections, configuration.ParallelModeOrDefault);
		Assert.Null(configuration.PreEnumerateTheories);
		Assert.Null(configuration.PrintMaxEnumerableLength);
		Assert.Null(configuration.PrintMaxObjectDepth);
		Assert.Null(configuration.PrintMaxObjectMemberCount);
		Assert.Null(configuration.PrintMaxStringLength);
		Assert.True(configuration.ShadowCopyOrDefault);
		Assert.False(configuration.ShowLiveOutputOrDefault);
		Assert.False(configuration.StopOnFailOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithValidValues_ReturnsConfiguredValues()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Configuration.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_OverrideValues.config"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(AppDomainSupport.Denied, configuration.AppDomainOrDefault);
		Assert.True(configuration.DiagnosticMessagesOrDefault);
		Assert.True(configuration.InternalDiagnosticMessagesOrDefault);
		Assert.Equal(5, configuration.LongRunningTestSecondsOrDefault);
		Assert.Equal(2112, configuration.MaxParallelThreadsOrDefault);
		Assert.Equal(TestMethodDisplay.Method, configuration.MethodDisplayOrDefault);
		Assert.Equal(TestMethodDisplayOptions.All, configuration.MethodDisplayOptionsOrDefault);
		Assert.Equal(ParallelAlgorithm.Aggressive, configuration.ParallelAlgorithmOrDefault);
		Assert.True(configuration.ParallelizeAssemblyOrDefault);
		Assert.Equal(ParallelMode.None, configuration.ParallelModeOrDefault);
		Assert.False(configuration.PreEnumerateTheories);
	}

	[Fact]
	public static void ConfigurationFileWithInvalidValues_FallsBackToDefaultValues()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Configuration.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_BadValues.config"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(AppDomainSupport.IfAvailable, configuration.AppDomainOrDefault);
		Assert.Null(configuration.AssertEquivalentMaxDepth);
		Assert.False(configuration.DiagnosticMessagesOrDefault);
		Assert.False(configuration.FailSkipsOrDefault);
		Assert.False(configuration.InternalDiagnosticMessagesOrDefault);
		Assert.Equal(-1, configuration.LongRunningTestSecondsOrDefault);
		Assert.Equal(Environment.ProcessorCount, configuration.MaxParallelThreadsOrDefault);
		Assert.Equal(TestMethodDisplay.ClassAndMethod, configuration.MethodDisplayOrDefault);
		Assert.Equal(TestMethodDisplayOptions.None, configuration.MethodDisplayOptionsOrDefault);
		Assert.Equal(ParallelAlgorithm.Conservative, configuration.ParallelAlgorithmOrDefault);
		// This value was valid as a sentinel to make sure we were trying to read values from the config file
		Assert.True(configuration.ParallelizeAssemblyOrDefault);
		Assert.Equal(ParallelMode.Collections, configuration.ParallelModeOrDefault);
		Assert.Null(configuration.PreEnumerateTheories);
		Assert.Null(configuration.PrintMaxEnumerableLength);
		Assert.Null(configuration.PrintMaxObjectDepth);
		Assert.Null(configuration.PrintMaxObjectMemberCount);
		Assert.Null(configuration.PrintMaxStringLength);
		Assert.True(configuration.ShadowCopyOrDefault);
		Assert.False(configuration.ShowLiveOutputOrDefault);
		Assert.False(configuration.StopOnFailOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithNegativeThreadValue_ReturnsConfiguredValue()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Configuration.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_MaxThreadsNegativeOne.config"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(-1, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithZeroThreads_ReturnsProcessorCount()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Configuration.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_MaxThreadsZero.config"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(Environment.ProcessorCount, configuration.MaxParallelThreadsOrDefault);
	}
}

#endif  // NETFRAMEWORK
