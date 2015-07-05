using System;
using System.IO;
using System.Reflection;
using Xunit;

public class ConfigReader_dnxTests
{
    public class Load
    {
        static readonly string assemblyPath;

        static Load()
        {
            assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetLocalCodeBase());
        }

        [Fact]
        public static void EmptyConfigurationFile_ReturnsDefaultValues()
        {
            var result = ConfigReader_dnx.Load(null, Path.Combine(assemblyPath, "ConfigReader_Empty.json"));

            Assert.False(result.DiagnosticMessagesOrDefault);
            Assert.Equal(Environment.ProcessorCount, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.ClassAndMethod, result.MethodDisplayOrDefault);
            Assert.False(result.ParallelizeAssemblyOrDefault);
            Assert.True(result.ParallelizeTestCollectionsOrDefault);
            Assert.True(result.PreEnumerateTheoriesOrDefault);
        }

        [Fact]
        public static void ConfigurationFileWithValidValues_ReturnsConfiguredValues()
        {
            var result = ConfigReader_dnx.Load(null, Path.Combine(assemblyPath, "ConfigReader_OverrideValues.json"));

            Assert.True(result.DiagnosticMessagesOrDefault);
            Assert.Equal(2112, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.Method, result.MethodDisplayOrDefault);
            Assert.True(result.ParallelizeAssemblyOrDefault);
            Assert.False(result.ParallelizeTestCollectionsOrDefault);
            Assert.False(result.PreEnumerateTheoriesOrDefault);
        }

        [Fact]
        public static void ConfigurationFileWithInvalidValues_FallsBackToDefaultValues()
        {
            var result = ConfigReader_dnx.Load(null, Path.Combine(assemblyPath, "ConfigReader_BadValues.json"));

            Assert.False(result.DiagnosticMessagesOrDefault);
            Assert.Equal(Environment.ProcessorCount, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.ClassAndMethod, result.MethodDisplayOrDefault);
            // This value was valid as a sentinel to make sure we were trying to read values from the JSON
            Assert.True(result.ParallelizeAssemblyOrDefault);
            Assert.True(result.ParallelizeTestCollectionsOrDefault);
            Assert.True(result.PreEnumerateTheoriesOrDefault);
        }
    }
}
