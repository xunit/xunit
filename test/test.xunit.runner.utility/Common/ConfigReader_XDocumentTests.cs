using System;
using System.IO;
using System.Reflection;
using Xunit;
using ConfigReader = Xunit.ConfigReader_XDocument;

public class ConfigReader_XDocumentTests
{
    public class Load
    {
        static readonly string assemblyFileName;

        static Load()
        {
            assemblyFileName = Assembly.GetExecutingAssembly().GetLocalCodeBase();
        }

        [Fact]
        public static void NoConfigurationFile_ReturnsDefaultValues()
        {
            var result = ConfigReader.Load(assemblyFileName);

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
            var result = ConfigReader.Load(assemblyFileName, Path.Combine(Path.GetDirectoryName(assemblyFileName), "ConfigReader_OverrideValues.config"));

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
            var result = ConfigReader.Load(assemblyFileName, Path.Combine(Path.GetDirectoryName(assemblyFileName), "ConfigReader_BadValues.config"));

            Assert.False(result.DiagnosticMessagesOrDefault);
            Assert.Equal(Environment.ProcessorCount, result.MaxParallelThreadsOrDefault);
            Assert.Equal(TestMethodDisplay.ClassAndMethod, result.MethodDisplayOrDefault);
            // This value was valid as a sentinel to make sure we were trying to read values from the file
            Assert.True(result.ParallelizeAssemblyOrDefault);
            Assert.True(result.ParallelizeTestCollectionsOrDefault);
            Assert.True(result.PreEnumerateTheoriesOrDefault);
        }
    }
}
