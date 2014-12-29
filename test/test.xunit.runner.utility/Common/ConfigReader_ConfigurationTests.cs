using System;
using System.IO;
using System.Reflection;
using Xunit;

public class ConfigReader_ConfigurationTests
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

            Assert.False(result.DiagnosticMessages);
            Assert.Equal(Environment.ProcessorCount, result.MaxParallelThreads);
            Assert.Equal(TestMethodDisplay.ClassAndMethod, result.MethodDisplay);
            Assert.False(result.ParallelizeAssembly);
            Assert.True(result.ParallelizeTestCollections);
            Assert.True(result.PreEnumerateTheories);
        }

        [Fact]
        public static void ConfigurationFileWithValidValues_ReturnsConfiguredValues()
        {
            var result = ConfigReader.Load(assemblyFileName, Path.Combine(Path.GetDirectoryName(assemblyFileName), "ConfigReader_OverrideValues.config"));

            Assert.True(result.DiagnosticMessages);
            Assert.Equal(2112, result.MaxParallelThreads);
            Assert.Equal(TestMethodDisplay.Method, result.MethodDisplay);
            Assert.True(result.ParallelizeAssembly);
            Assert.False(result.ParallelizeTestCollections);
            Assert.False(result.PreEnumerateTheories);
        }

        [Fact]
        public static void ConfigurationFileWithInvalidValues_FallsBackToDefaultValues()
        {
            var result = ConfigReader.Load(assemblyFileName, Path.Combine(Path.GetDirectoryName(assemblyFileName), "ConfigReader_BadValues.config"));

            Assert.False(result.DiagnosticMessages);
            Assert.Equal(Environment.ProcessorCount, result.MaxParallelThreads);
            Assert.Equal(TestMethodDisplay.ClassAndMethod, result.MethodDisplay);
            // This value was valid as a sentinel to make sure we were trying to read values from the file
            Assert.True(result.ParallelizeAssembly);
            Assert.True(result.ParallelizeTestCollections);
            Assert.True(result.PreEnumerateTheories);
        }
    }
}
