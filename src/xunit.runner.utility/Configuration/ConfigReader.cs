namespace Xunit
{
    /// <summary>
    /// This class is used to read configuration information for a test assembly.
    /// </summary>
    public static class ConfigReader
    {
        /// <summary>
        /// Loads the test assembly configuration for the given test assembly.
        /// </summary>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <returns>The test assembly configuration.</returns>
        public static TestAssemblyConfiguration Load(string assemblyFileName, string configFileName = null)
        {
            return ConfigReader_Json.Load(assemblyFileName, configFileName)
#if NET35
                ?? ConfigReader_Configuration.Load(assemblyFileName, configFileName)
#endif
                ?? new TestAssemblyConfiguration();
        }
    }
}
