using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Xunit;

public class ConfigReaderTests
{
    static readonly string AssemblyFileName;
    static readonly string AssemblyPath;

    static ConfigReaderTests()
    {
        AssemblyFileName = Assembly.GetExecutingAssembly().GetLocalCodeBase();
        AssemblyPath = Path.GetDirectoryName(AssemblyFileName);
    }

    [Theory]
    [InlineData("UnknownFile.json")]
#if NETFRAMEWORK
    [InlineData("UnknownFile.config")]
#endif
    public static void ConfigurationFileNotFound_ReturnsWarning(string configFileName)
    {
        var configFilePath = Path.Combine(AssemblyPath, configFileName);
        var warnings = new List<string>();

        ConfigReader.Load(AssemblyFileName, configFilePath, warnings);

        var warning = Assert.Single(warnings);
        Assert.Equal($"Couldn't load config file '{configFilePath}': file not found", warning);
    }

    [Theory]
    [InlineData("UnknownFile.xml")]
#if !NETFRAMEWORK
    [InlineData("UnknownFile.config")]
#endif
    public static void ConfigurationFileTypeUnknown_ReturnsWarning(string configFileName)
    {
        var configFilePath = Path.Combine(AssemblyPath, configFileName);
        var warnings = new List<string>();

        ConfigReader.Load(AssemblyFileName, configFilePath, warnings);

        var warning = Assert.Single(warnings);
        Assert.Equal($"Couldn't load config file '{configFilePath}': unknown file type", warning);
    }

    public class Load_WithJson
    {
        [Fact]
        public static void EmptyConfigurationFile_ReturnsDefaultValues()
        {
            var warnings = new List<string>();

            var result = ConfigReader.Load(AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_Empty.json"), warnings);

            Assert.Empty(warnings);
            Assert.False(result.DiagnosticMessagesOrDefault);
            Assert.False(result.FailSkipsOrDefault);
            Assert.False(result.InternalDiagnosticMessagesOrDefault);
            Assert.Equal(Environment.ProcessorCount, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.ClassAndMethod, result.MethodDisplayOrDefault);
            Assert.Equal(TestMethodDisplayOptions.None, result.MethodDisplayOptionsOrDefault);
            Assert.Equal(ParallelAlgorithm.Conservative, result.ParallelAlgorithmOrDefault);
            Assert.False(result.ParallelizeAssemblyOrDefault);
            Assert.True(result.ParallelizeTestCollectionsOrDefault);
            Assert.True(result.PreEnumerateTheoriesOrDefault);
        }

        [Fact]
        public static void ConfigurationFileWithValidValues_ReturnsConfiguredValues()
        {
            var warnings = new List<string>();

            var result = ConfigReader.Load(AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_OverrideValues.json"), warnings);

            Assert.Empty(warnings);
            Assert.True(result.DiagnosticMessagesOrDefault);
            Assert.True(result.FailSkipsOrDefault);
            Assert.True(result.InternalDiagnosticMessagesOrDefault);
            Assert.Equal(2112, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.Method, result.MethodDisplayOrDefault);
            Assert.Equal(TestMethodDisplayOptions.All, result.MethodDisplayOptionsOrDefault);
            Assert.Equal(ParallelAlgorithm.Aggressive, result.ParallelAlgorithmOrDefault);
            Assert.True(result.ParallelizeAssemblyOrDefault);
            Assert.False(result.ParallelizeTestCollectionsOrDefault);
            Assert.False(result.PreEnumerateTheoriesOrDefault);
            Assert.Equal(5, result.LongRunningTestSecondsOrDefault);
        }

        [Fact]
        public static void ConfigurationFileWithInvalidValues_FallsBackToDefaultValues()
        {
            var warnings = new List<string>();

            var result = ConfigReader.Load(AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_BadValues.json"), warnings);

            Assert.Empty(warnings);
            Assert.False(result.DiagnosticMessagesOrDefault);
            Assert.False(result.FailSkipsOrDefault);
            Assert.False(result.InternalDiagnosticMessagesOrDefault);
            Assert.Equal(Environment.ProcessorCount, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.ClassAndMethod, result.MethodDisplayOrDefault);
            Assert.Equal(TestMethodDisplayOptions.None, result.MethodDisplayOptionsOrDefault);
            Assert.Equal(ParallelAlgorithm.Conservative, result.ParallelAlgorithmOrDefault);
            // This value was valid as a sentinel to make sure we were trying to read values from the JSON
            Assert.True(result.ParallelizeAssemblyOrDefault);
            Assert.True(result.ParallelizeTestCollectionsOrDefault);
            Assert.True(result.PreEnumerateTheoriesOrDefault);
        }

        [Fact]
        public static void ConfigurationFileWithNegativeThreadValue_ReturnsConfiguredValue()
        {
            var result = ConfigReader.Load(AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_NegativeThreads.json"));

            Assert.Equal(-1, result.MaxParallelThreadsOrDefault);
        }

        [Fact]
        public static void ConfigurationFileWithNonTopLevelObject_ReturnsWarning()
        {
            var warnings = new List<string>();
            var configFilePath = Path.Combine(AssemblyPath, "ConfigReader_NonObject.json");

            ConfigReader.Load(AssemblyFileName, configFilePath, warnings);

            var warning = Assert.Single(warnings);
            Assert.Equal($"Couldn't parse config file '{configFilePath}': the root must be a JSON object", warning);
        }

        [Fact]
        public static void InvalidJson_ReturnsWarning()
        {
            var warnings = new List<string>();
            var configFilePath = Path.Combine(AssemblyPath, "ConfigReader_InvalidJson.json");

            ConfigReader.Load(AssemblyFileName, configFilePath, warnings);

            var warning = Assert.Single(warnings);
            Assert.Equal($"Exception loading config file '{configFilePath}': Illegal character '?' (Unicode hexadecimal 003F).", warning);
        }
    }

#if NETFRAMEWORK
    public class Load_WithXml
    {
        [Fact]
        public static void EmptyConfigurationFile_ReturnsDefaultValues()
        {
            var warnings = new List<string>();

            var result = ConfigReader.Load(AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_Empty.config"), warnings);

            Assert.Empty(warnings);
            Assert.False(result.DiagnosticMessagesOrDefault);
            Assert.False(result.InternalDiagnosticMessagesOrDefault);
            Assert.Equal(Environment.ProcessorCount, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.ClassAndMethod, result.MethodDisplayOrDefault);
            Assert.Equal(TestMethodDisplayOptions.None, result.MethodDisplayOptionsOrDefault);
            Assert.Equal(ParallelAlgorithm.Conservative, result.ParallelAlgorithmOrDefault);
            Assert.False(result.ParallelizeAssemblyOrDefault);
            Assert.True(result.ParallelizeTestCollectionsOrDefault);
            Assert.True(result.PreEnumerateTheoriesOrDefault);
        }

        [Fact]
        public static void ConfigurationFileWithValidValues_ReturnsConfiguredValues()
        {
            var warnings = new List<string>();

            var result = ConfigReader.Load(AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_OverrideValues.config"), warnings);

            Assert.Empty(warnings);
            Assert.True(result.DiagnosticMessagesOrDefault);
            Assert.True(result.InternalDiagnosticMessagesOrDefault);
            Assert.Equal(2112, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.Method, result.MethodDisplayOrDefault);
            Assert.Equal(TestMethodDisplayOptions.All, result.MethodDisplayOptionsOrDefault);
            Assert.Equal(ParallelAlgorithm.Aggressive, result.ParallelAlgorithmOrDefault);
            Assert.True(result.ParallelizeAssemblyOrDefault);
            Assert.False(result.ParallelizeTestCollectionsOrDefault);
            Assert.False(result.PreEnumerateTheoriesOrDefault);
            Assert.Equal(5, result.LongRunningTestSecondsOrDefault);
        }

        [Fact]
        public static void ConfigurationFileWithInvalidValues_FallsBackToDefaultValues()
        {
            var warnings = new List<string>();

            var result = ConfigReader.Load(AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_BadValues.config"), warnings);

            Assert.Empty(warnings);
            Assert.False(result.DiagnosticMessagesOrDefault);
            Assert.False(result.InternalDiagnosticMessagesOrDefault);
            Assert.Equal(Environment.ProcessorCount, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.ClassAndMethod, result.MethodDisplayOrDefault);
            Assert.Equal(TestMethodDisplayOptions.None, result.MethodDisplayOptionsOrDefault);
            Assert.Equal(ParallelAlgorithm.Conservative, result.ParallelAlgorithmOrDefault);
            // This value was valid as a sentinel to make sure we were trying to read values from the file
            Assert.True(result.ParallelizeAssemblyOrDefault);
            Assert.True(result.ParallelizeTestCollectionsOrDefault);
            Assert.True(result.PreEnumerateTheoriesOrDefault);
        }

        [Fact]
        public static void ConfigurationFileWithNegativeThreadValue_ReturnsConfiguredValue()
        {
            var result = ConfigReader.Load(AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_NegativeThreads.config"));

            Assert.Equal(-1, result.MaxParallelThreadsOrDefault);
        }
    }
#endif
}
