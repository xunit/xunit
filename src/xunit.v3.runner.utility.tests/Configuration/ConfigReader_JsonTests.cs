using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public static class ConfigReader_JsonTests
{
	static readonly string AssemblyPath;

	static ConfigReader_JsonTests() =>
		AssemblyPath = AppContext.BaseDirectory;

	[Fact]
	public static void EmptyConfigurationFile_ReturnsDefaultValues()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, /* lang=json */ "{}", warnings);

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
		Assert.Equal(ParallelMode.Collections, configuration.ParallelModeOrDefault);
		Assert.Null(configuration.PreEnumerateTheories);
		Assert.True(configuration.ShadowCopyOrDefault);
		Assert.False(configuration.ShowLiveOutputOrDefault);
		Assert.Equal(10, configuration.ShutdownForegroundThreadWaitSecondsOrDefault);
		Assert.False(configuration.StopOnFailOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithValidValues_ReturnsConfiguredValues()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, /* lang=json */ """
			{
			  "appDomain": "denied",
			  "assertEquivalentMaxDepth": 100,
			  "culture": "en-GB",
			  "diagnosticMessages": true,
			  "failSkips": true,
			  "internalDiagnosticMessages": true,
			  "longRunningTestSeconds": 5,
			  "maxParallelThreads": 2112,
			  "methodDisplay": "method",
			  "methodDisplayOptions": "all",
			  "parallelAlgorithm": "aggressive",
			  "parallelizeAssembly": true,
			  "parallelmode": "all",
			  "preEnumerateTheories": false,
			  "printMaxEnumerableLength": 200,
			  "printMaxObjectDepth": 300,
			  "printMaxObjectMemberCount": 400,
			  "printMaxStringLength": 500,
			  "shadowCopy": false,
			  "showLiveOutput": true,
			  "shutdownForegroundThreadWaitSeconds":  42,
			  "stopOnFail": true
			}
			""", warnings);

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
		Assert.Equal(ParallelMode.All, configuration.ParallelModeOrDefault);
		Assert.False(configuration.PreEnumerateTheories);
		Assert.True(configuration.ShowLiveOutputOrDefault);
		Assert.Equal(42, configuration.ShutdownForegroundThreadWaitSecondsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithInvalidValues_FallsBackToDefaultValues()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, /* lang=json */ """
			{
			  "appDomain": "blarch",
			  "assertEquivalentMaxDepth": 0,
			  "diagnosticMessages": "blarch",
			  "internalDiagnosticMessages": "blarch",
			  "longRunningTestSeconds": "blarch",
			  "maxParallelThreads": "abc",
			  "methodDisplay": "fooBar",
			  "methodDisplayOptions": "fooBar",
			  "parallelAlgorithm": "blarch",
			  "parallelizeAssembly": true,
			  "parallelmode": "biff",
			  "preEnumerateTheories": "baz",
			  "printMaxEnumerableLength": -1,
			  "printMaxObjectDepth": -1,
			  "printMaxObjectMemberCount": -1,
			  "printMaxStringLength": -1,
			  "shadowCopy": "blarch",
			  "showLiveOutput": "blarch",
			  "shutdownForegroundThreadWaitSeconds": 0,
			  "stopOnFail": "blarch"
			}
			""", warnings);

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
		Assert.Equal(ParallelMode.Collections, configuration.ParallelModeOrDefault);
		Assert.Null(configuration.PreEnumerateTheories);
		Assert.True(configuration.ShadowCopyOrDefault);
		Assert.False(configuration.ShowLiveOutputOrDefault);
		Assert.Equal(10, configuration.ShutdownForegroundThreadWaitSecondsOrDefault);
		Assert.False(configuration.StopOnFailOrDefault);
	}

	[Fact]
	public static void SupportDefaultCulture()
	{
		var configuration = new TestAssemblyConfiguration { Culture = "override-me" };
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, /* lang=json */ """
			{
			  "culture": "default"
			}
			""", warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Null(configuration.Culture);
	}

	[Fact]
	public static void SupportInvariantCulture()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, /* lang=json */ """
			{
			  "culture": "invariant"
			}
			""", warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(string.Empty, configuration.Culture);
	}

	[Fact]
	public static void ConfigurationFileWithNegativeThreadValue_ReturnsConfiguredValue()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, /* lang=json */ """
			{
			  "maxParallelThreads": -1
			}
			""", warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(-1, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithZeroThreads_ReturnsProcessorCount()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, /* lang=json */ """
			{
			  "maxParallelThreads": 0
			}
			""", warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(Environment.ProcessorCount, configuration.MaxParallelThreadsOrDefault);
	}

	[Theory]
	[InlineData("2x")]
	[InlineData("2.0x")]
	[InlineData("2,0x")]
	public static void ConfigurationFileWithMaxThreadsAsMultiplier_ReturnsMultipliedValue(string multiplier)
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, $$"""
			{
			  "maxParallelThreads": "{{multiplier}}"
			}
			""", warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(Environment.ProcessorCount * 2, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsExplicitDefault_ReturnsProcessorCount()
	{
		var configuration = new TestAssemblyConfiguration { MaxParallelThreads = 2112 };
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, /* lang=json */ """
			{
			  "maxParallelThreads": "default"
			}
			""", warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(Environment.ProcessorCount, configuration.MaxParallelThreadsOrDefault);
	}

	[Fact]
	public static void ConfigurationFileWithMaxThreadsExplicitUnlimited_ReturnsUnlimited()
	{
		var configuration = new TestAssemblyConfiguration();
		var warnings = new List<string>();

		var result = ConfigReader_Json.LoadFromJson(configuration, /* lang=json */ """
			{
			  "maxParallelThreads": "unlimited"
			}
			""", warnings);

		Assert.True(result);
		Assert.Empty(warnings);
		Assert.Equal(-1, configuration.MaxParallelThreadsOrDefault);
	}
}
