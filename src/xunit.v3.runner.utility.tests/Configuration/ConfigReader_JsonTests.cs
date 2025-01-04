using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class ConfigReader_JsonTests
{
	static readonly string AssemblyPath;

	static ConfigReader_JsonTests() =>
		AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetLocalCodeBase())!;

	[Fact]
	public static void EmptyConfigurationFile_ReturnsDefaultValues()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, LoadFile("ConfigReader_Empty.json"), warnings);

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

	[Fact]
	public static void ConfigurationFileWithValidValues_ReturnsConfiguredValues()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, LoadFile("ConfigReader_OverrideValues.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(AppDomainSupport.Denied, configuration.AppDomainOrDefault);
		Assert.Equal("en-GB", configuration.Culture);
		Assert.True(configuration.DiagnosticMessagesOrDefault);
		Assert.True(configuration.FailSkipsOrDefault);
		Assert.True(configuration.InternalDiagnosticMessagesOrDefault);
		Assert.Equal(5, configuration.LongRunningTestSecondsOrDefault);
		Assert.Equal(2112, configuration.MaxParallelThreadsOrDefault);
		Assert.Equal(TestMethodDisplay.Method, configuration.MethodDisplayOrDefault);
		Assert.Equal(TestMethodDisplayOptions.All, configuration.MethodDisplayOptionsOrDefault);
		Assert.Equal(ParallelAlgorithm.Aggressive, configuration.ParallelAlgorithmOrDefault);
		Assert.True(configuration.ParallelizeAssemblyOrDefault);
		Assert.False(configuration.ParallelizeTestCollectionsOrDefault);
		Assert.False(configuration.PreEnumerateTheories);
		Assert.True(configuration.ShowLiveOutputOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithInvalidValues_FallsBackToDefaultValues()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, LoadFile("ConfigReader_BadValues.json"), warnings);

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

		var result = ConfigReader_Json.LoadFromJson(configuration, LoadFile("ConfigReader_CultureDefault.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Null(configuration.Culture);
	}

	[Fact]
	public static void SupportInvariantCulture()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, LoadFile("ConfigReader_CultureInvariant.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(string.Empty, configuration.Culture);
	}

	[Fact]
	public static void ConfigurationFileWithNegativeThreadValue_ReturnsConfiguredValue()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, LoadFile("ConfigReader_MaxThreadsNegativeOne.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(-1, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithZeroThreads_ReturnsProcessorCount()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, LoadFile("ConfigReader_MaxThreadsZero.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(Environment.ProcessorCount, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsAsMultiplier_ReturnsMultipliedValue()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, LoadFile("ConfigReader_MaxThreadsMultiplier.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(Environment.ProcessorCount * 2, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsAsMultiplierWithComma_ReturnsMultipliedValue()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, LoadFile("ConfigReader_MaxThreadsMultiplierComma.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(Environment.ProcessorCount * 2, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsAsMultiplierWithDecimal_ReturnsMultipliedValue()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, LoadFile("ConfigReader_MaxThreadsMultiplierDecimal.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(Environment.ProcessorCount * 2, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsExplicitDefault_ReturnsProcessorCount()
	{
		var configuration = new TestAssemblyConfiguration { MaxParallelThreads = 2112 };
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, LoadFile("ConfigReader_MaxThreadsDefault.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(Environment.ProcessorCount, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsExplicitUnlimited_ReturnsUnlimited()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, LoadFile("ConfigReader_MaxThreadsUnlimited.json"), warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(-1, configuration.MaxParallelThreadsOrDefault);
	}

	static string LoadFile(string fileName) =>
		File.ReadAllText(Path.Combine(AssemblyPath, fileName));
}
