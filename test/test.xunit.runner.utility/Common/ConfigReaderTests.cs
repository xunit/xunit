using System;
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

    public class Load_WithJson
    {
        [Fact]
        public static void EmptyConfigurationFile_ReturnsDefaultValues()
        {
            var result = ConfigReader.Load(AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_Empty.json"));

            Assert.False(result.DiagnosticMessagesOrDefault);
            Assert.False(result.InternalDiagnosticMessagesOrDefault);
            Assert.Equal(Environment.ProcessorCount, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.ClassAndMethod, result.MethodDisplayOrDefault);
            Assert.Equal(TestMethodDisplayOptions.None, result.MethodDisplayOptionsOrDefault);
            Assert.False(result.ParallelizeAssemblyOrDefault);
            Assert.True(result.ParallelizeTestCollectionsOrDefault);
            Assert.True(result.PreEnumerateTheoriesOrDefault);
        }

        [Fact]
        public static void ConfigurationFileWithValidValues_ReturnsConfiguredValues()
        {
            var result = ConfigReader.Load(AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_OverrideValues.json"));

            Assert.True(result.DiagnosticMessagesOrDefault);
            Assert.True(result.InternalDiagnosticMessagesOrDefault);
            Assert.Equal(2112, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.Method, result.MethodDisplayOrDefault);
            Assert.Equal(TestMethodDisplayOptions.All, result.MethodDisplayOptionsOrDefault);
            Assert.True(result.ParallelizeAssemblyOrDefault);
            Assert.False(result.ParallelizeTestCollectionsOrDefault);
            Assert.False(result.PreEnumerateTheoriesOrDefault);
            Assert.Equal(5, result.LongRunningTestSecondsOrDefault);
        }

        [Fact]
        public static void ConfigurationFileWithInvalidValues_FallsBackToDefaultValues()
        {
            var result = ConfigReader.Load(AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_BadValues.json"));

            Assert.False(result.DiagnosticMessagesOrDefault);
            Assert.False(result.InternalDiagnosticMessagesOrDefault);
            Assert.Equal(Environment.ProcessorCount, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.ClassAndMethod, result.MethodDisplayOrDefault);
            Assert.Equal(TestMethodDisplayOptions.None, result.MethodDisplayOptionsOrDefault);
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
    }

#if NETFRAMEWORK
    public class Load_WithXml
    {
        [Fact]
        public static void EmptyConfigurationFile_ReturnsDefaultValues()
        {
            var result = ConfigReader.Load(AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_Empty.config"));

            Assert.False(result.DiagnosticMessagesOrDefault);
            Assert.False(result.InternalDiagnosticMessagesOrDefault);
            Assert.Equal(Environment.ProcessorCount, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.ClassAndMethod, result.MethodDisplayOrDefault);
            Assert.Equal(TestMethodDisplayOptions.None, result.MethodDisplayOptionsOrDefault);
            Assert.False(result.ParallelizeAssemblyOrDefault);
            Assert.True(result.ParallelizeTestCollectionsOrDefault);
            Assert.True(result.PreEnumerateTheoriesOrDefault);
        }

        [Fact]
        public static void ConfigurationFileWithValidValues_ReturnsConfiguredValues()
        {
            var result = ConfigReader.Load(AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_OverrideValues.config"));

            Assert.True(result.DiagnosticMessagesOrDefault);
            Assert.True(result.InternalDiagnosticMessagesOrDefault);
            Assert.Equal(2112, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.Method, result.MethodDisplayOrDefault);
            Assert.Equal(TestMethodDisplayOptions.All, result.MethodDisplayOptionsOrDefault);
            Assert.True(result.ParallelizeAssemblyOrDefault);
            Assert.False(result.ParallelizeTestCollectionsOrDefault);
            Assert.False(result.PreEnumerateTheoriesOrDefault);
            Assert.Equal(5, result.LongRunningTestSecondsOrDefault);
        }

        [Fact]
        public static void ConfigurationFileWithInvalidValues_FallsBackToDefaultValues()
        {
            var result = ConfigReader.Load(AssemblyFileName, Path.Combine(AssemblyPath, "ConfigReader_BadValues.config"));

            Assert.False(result.DiagnosticMessagesOrDefault);
            Assert.False(result.InternalDiagnosticMessagesOrDefault);
            Assert.Equal(Environment.ProcessorCount, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.ClassAndMethod, result.MethodDisplayOrDefault);
            Assert.Equal(TestMethodDisplayOptions.None, result.MethodDisplayOptionsOrDefault);
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
