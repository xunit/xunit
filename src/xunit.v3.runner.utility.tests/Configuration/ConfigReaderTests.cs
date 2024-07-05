using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class ConfigReaderTests
{
	static readonly string AssemblyFileName;
	static readonly string AssemblyPath;

	static ConfigReaderTests()
	{
		AssemblyFileName = Assembly.GetExecutingAssembly().GetLocalCodeBase();
		AssemblyPath = Path.GetDirectoryName(AssemblyFileName)!;
	}

	[Theory]
	[InlineData("UnknownFile.json")]
#if NETFRAMEWORK
	[InlineData("UnknownFile.config")]
#endif
	public static void ConfigurationFileNotFound_ReturnsFalseWithWarning(string configFileName)
	{
		var configuration = new TestAssemblyConfiguration();
		var configFilePath = Path.Combine(AssemblyPath, configFileName);
		var warnings = new List<string>();

		var result = ConfigReader.Load(configuration, AssemblyFileName, configFilePath, warnings);

		Assert.False(result);
		var warning = Assert.Single(warnings);
		Assert.Equal($"Couldn't load config file '{configFilePath}': file not found", warning);
	}

	[Theory]
	[InlineData("UnknownFile.xml")]
#if !NETFRAMEWORK
	[InlineData("UnknownFile.config")]
#endif
	public static void ConfigurationFileTypeUnknown_ReturnsFalseWithWarning(string configFileName)
	{
		var configuration = new TestAssemblyConfiguration();
		var configFilePath = Path.Combine(AssemblyPath, configFileName);
		var warnings = new List<string>();

		var result = ConfigReader.Load(configuration, AssemblyFileName, configFilePath, warnings);

		Assert.False(result);
		var warning = Assert.Single(warnings);
		Assert.Equal($"Couldn't load config file '{configFilePath}': unknown file type", warning);
	}

	[Theory]
	[InlineData("ConfigReader_Empty.json")]
#if NETFRAMEWORK
	[InlineData("ConfigReader_Empty.config")]
#endif
	public static void EmptyConfigurationFile_ReturnsDefaultValues(string configFileName)
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, configFileName), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(AppDomainSupport.IfAvailable, configuration.AppDomainOrDefault);
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
		Assert.True(configuration.ParallelizeTestCollectionsOrDefault);
		Assert.Null(configuration.PreEnumerateTheories);
		Assert.True(configuration.ShadowCopyOrDefault);
		Assert.False(configuration.ShowLiveOutputOrDefault);
		Assert.False(configuration.StopOnFailOrDefault);
	}

	[Theory]
	[InlineData("ConfigReader_OverrideValues.json")]
#if NETFRAMEWORK
	[InlineData("ConfigReader_OverrideValues.config")]
#endif
	public static void ConfigurationFileWithValidValues_ReturnsConfiguredValues(string configFileName)
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, configFileName), warnings);

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
		Assert.False(configuration.ParallelizeTestCollectionsOrDefault);
		Assert.False(configuration.PreEnumerateTheories);

		if (configFileName.EndsWith(".json"))
		{
			Assert.Equal("en-GB", configuration.Culture);
			Assert.True(configuration.FailSkipsOrDefault);
			Assert.True(configuration.ShowLiveOutputOrDefault);
		}
	}

	[Theory]
	[InlineData("ConfigReader_BadValues.json")]
#if NETFRAMEWORK
	[InlineData("ConfigReader_BadValues.config")]
#endif
	public static void ConfigurationFileWithInvalidValues_FallsBackToDefaultValues(string configFileName)
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, configFileName), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(AppDomainSupport.IfAvailable, configuration.AppDomainOrDefault);
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
		Assert.True(configuration.ParallelizeTestCollectionsOrDefault);
		Assert.Null(configuration.PreEnumerateTheories);
		Assert.True(configuration.ShadowCopyOrDefault);
		Assert.False(configuration.ShowLiveOutputOrDefault);
		Assert.False(configuration.StopOnFailOrDefault);
	}

	[Fact]
	public static void SupportDefaultCulture()
	{
		var configuration = new TestAssemblyConfiguration { Culture = "override-me" };
		var warnings = new List<string>();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_CultureDefault.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Null(configuration.Culture);
	}

	[Fact]
	public static void SupportInvariantCulture()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_CultureInvariant.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(string.Empty, configuration.Culture);
	}

	[Theory]
	[InlineData("ConfigReader_MaxThreadsNegativeOne.json")]
#if NETFRAMEWORK
	[InlineData("ConfigReader_MaxThreadsNegativeOne.config")]
#endif
	public static void ConfigurationFileWithNegativeThreadValue_ReturnsConfiguredValue(string configFileName)
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, configFileName), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(-1, configuration.MaxParallelThreadsOrDefault);
	}

	[Theory]
	[InlineData("ConfigReader_MaxThreadsZero.json")]
#if NETFRAMEWORK
	[InlineData("ConfigReader_MaxThreadsZero.config")]
#endif
	public static void ConfigurationFileWithZeroThreads_ReturnsProcessorCount(string configFileName)
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, configFileName), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(Environment.ProcessorCount, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsAsMultiplier_ReturnsMultipliedValue()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_MaxThreadsMultiplier.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(Environment.ProcessorCount * 2, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsAsMultiplierWithComma_ReturnsMultipliedValue()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_MaxThreadsMultiplierComma.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(Environment.ProcessorCount * 2, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsAsMultiplierWithDecimal_ReturnsMultipliedValue()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_MaxThreadsMultiplierDecimal.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(Environment.ProcessorCount * 2, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsExplicitDefault_ReturnsProcessorCount()
	{
		var configuration = new TestAssemblyConfiguration { MaxParallelThreads = 2112 };
		var warnings = new List<string>();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_MaxThreadsDefault.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(Environment.ProcessorCount, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsExplicitUnlimited_ReturnsUnlimited()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_MaxThreadsUnlimited.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(-1, configuration.MaxParallelThreadsOrDefault);
	}
}
