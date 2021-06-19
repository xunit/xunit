using System;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.v3;

public class ConfigReaderTests
{
	static readonly string AssemblyFileName;
	static readonly string AssemblyPath;

	static ConfigReaderTests()
	{
		AssemblyFileName = Assembly.GetExecutingAssembly().GetLocalCodeBase();
		AssemblyPath = Path.GetDirectoryName(AssemblyFileName)!;
	}

	[Fact]
	public static void ConfigurationFileNotFound_ReturnsFalse()
	{
		var configuration = new TestAssemblyConfiguration();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "UnknownFile.txt"));

		Assert.False(result);
	}

	[Theory]
	[InlineData("ConfigReader_Empty.json")]
#if NETFRAMEWORK
	[InlineData("ConfigReader_Empty.config")]
#endif
	public static void EmptyConfigurationFile_ReturnsDefaultValues(string configFileName)
	{
		var configuration = new TestAssemblyConfiguration();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, configFileName));

		Assert.True(result);
		Assert.False(configuration.DiagnosticMessagesOrDefault);
		Assert.False(configuration.InternalDiagnosticMessagesOrDefault);
		Assert.Equal(Environment.ProcessorCount, configuration.MaxParallelThreadsOrDefault);
		Assert.Equal(TestMethodDisplay.ClassAndMethod, configuration.MethodDisplayOrDefault);
		Assert.Equal(TestMethodDisplayOptions.None, configuration.MethodDisplayOptionsOrDefault);
		Assert.False(configuration.ParallelizeAssemblyOrDefault);
		Assert.True(configuration.ParallelizeTestCollectionsOrDefault);
		Assert.True(configuration.PreEnumerateTheoriesOrDefault);

		if (configFileName.EndsWith(".json"))
			Assert.False(configuration.FailSkipsOrDefault);
	}

	[Theory]
	[InlineData("ConfigReader_OverrideValues.json")]
#if NETFRAMEWORK
	[InlineData("ConfigReader_OverrideValues.config")]
#endif
	public static void ConfigurationFileWithValidValues_ReturnsConfiguredValues(string configFileName)
	{
		var configuration = new TestAssemblyConfiguration();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, configFileName));

		Assert.True(result);
		Assert.True(configuration.DiagnosticMessagesOrDefault);
		Assert.True(configuration.InternalDiagnosticMessagesOrDefault);
		Assert.Equal(2112, configuration.MaxParallelThreadsOrDefault);
		Assert.Equal(TestMethodDisplay.Method, configuration.MethodDisplayOrDefault);
		Assert.Equal(TestMethodDisplayOptions.All, configuration.MethodDisplayOptionsOrDefault);
		Assert.True(configuration.ParallelizeAssemblyOrDefault);
		Assert.False(configuration.ParallelizeTestCollectionsOrDefault);
		Assert.False(configuration.PreEnumerateTheoriesOrDefault);
		Assert.Equal(5, configuration.LongRunningTestSecondsOrDefault);

		if (configFileName.EndsWith(".json"))
			Assert.True(configuration.FailSkipsOrDefault);
	}

	[Theory]
	[InlineData("ConfigReader_BadValues.json")]
#if NETFRAMEWORK
	[InlineData("ConfigReader_BadValues.config")]
#endif
	public static void ConfigurationFileWithInvalidValues_FallsBackToDefaultValues(string configFileName)
	{
		var configuration = new TestAssemblyConfiguration();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, configFileName));

		Assert.True(result);
		Assert.False(configuration.DiagnosticMessagesOrDefault);
		Assert.False(configuration.InternalDiagnosticMessagesOrDefault);
		Assert.Equal(Environment.ProcessorCount, configuration.MaxParallelThreadsOrDefault);
		Assert.Equal(TestMethodDisplay.ClassAndMethod, configuration.MethodDisplayOrDefault);
		Assert.Equal(TestMethodDisplayOptions.None, configuration.MethodDisplayOptionsOrDefault);
		// This value was valid as a sentinel to make sure we were trying to read values from the config file
		Assert.True(configuration.ParallelizeAssemblyOrDefault);
		Assert.True(configuration.ParallelizeTestCollectionsOrDefault);
		Assert.True(configuration.PreEnumerateTheoriesOrDefault);

		if (configFileName.EndsWith(".json"))
			Assert.False(configuration.FailSkipsOrDefault);
	}

	[Theory]
	[InlineData("ConfigReader_MaxThreadsNegativeOne.json")]
#if NETFRAMEWORK
	[InlineData("ConfigReader_MaxThreadsNegativeOne.config")]
#endif
	public static void ConfigurationFileWithNegativeThreadValue_ReturnsConfiguredValue(string configFileName)
	{
		var configuration = new TestAssemblyConfiguration();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, configFileName));

		Assert.True(result);
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

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, configFileName));

		Assert.True(result);
		Assert.Equal(Environment.ProcessorCount, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsAsMultiplier_ReturnsMultipliedValue()
	{
		var configuration = new TestAssemblyConfiguration();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_MaxThreadsMultiplier.json"));

		Assert.True(result);
		Assert.Equal(Environment.ProcessorCount * 2, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsExplicitDefault_ReturnsProcessorCount()
	{
		var configuration = new TestAssemblyConfiguration { MaxParallelThreads = 2112 };

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_MaxThreadsDefault.json"));

		Assert.True(result);
		Assert.Equal(Environment.ProcessorCount, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsExplicitUnlimited_ReturnsUnlimited()
	{
		var configuration = new TestAssemblyConfiguration();

		var result = ConfigReader.Load(configuration, AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_MaxThreadsUnlimited.json"));

		Assert.True(result);
		Assert.Equal(-1, configuration.MaxParallelThreadsOrDefault);
	}
}
