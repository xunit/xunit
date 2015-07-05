using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Xunit
{
    /// <summary>
    /// This class is used to read configuration information for a test assembly.
    /// </summary>
#if UNIT_TEST
    public static class ConfigReader_dnx
#else
    public static class ConfigReader
#endif
    {
        /// <summary>
        /// Loads the test assembly configuration for the given test assembly.
        /// </summary>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <returns>The test assembly configuration.</returns>
        public static TestAssemblyConfiguration Load(string assemblyFileName, string configFileName = null)
        {
            if (configFileName == null)
                configFileName = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.runner.dnx.json");

            var result = new TestAssemblyConfiguration();

            if (File.Exists(configFileName))
            {
                try
                {
                    var config = JObject.Parse(File.ReadAllText(configFileName));

                    foreach (var property in config.Properties())
                    {
                        if (string.Equals(property.Name, Configuration.DiagnosticMessages, StringComparison.OrdinalIgnoreCase) && property.Value.Type == JTokenType.Boolean)
                            result.DiagnosticMessages = (bool)property.Value;
                        if (string.Equals(property.Name, Configuration.MaxParallelThreads, StringComparison.OrdinalIgnoreCase) && property.Value.Type == JTokenType.Integer)
                        {
                            var maxParallelThreads = (int)property.Value;
                            if (maxParallelThreads > 0)
                                result.MaxParallelThreads = maxParallelThreads;
                        }
                        if (string.Equals(property.Name, Configuration.MethodDisplay, StringComparison.OrdinalIgnoreCase) && property.Value.Type == JTokenType.String)
                        {
                            TestMethodDisplay methodDisplay;
                            if (Enum.TryParse((string)property.Value, true, out methodDisplay))
                                result.MethodDisplay = methodDisplay;
                        }
                        if (string.Equals(property.Name, Configuration.ParallelizeAssembly, StringComparison.OrdinalIgnoreCase) && property.Value.Type == JTokenType.Boolean)
                            result.ParallelizeAssembly = (bool)property.Value;
                        if (string.Equals(property.Name, Configuration.ParallelizeTestCollections, StringComparison.OrdinalIgnoreCase) && property.Value.Type == JTokenType.Boolean)
                            result.ParallelizeTestCollections = (bool)property.Value;
                        if (string.Equals(property.Name, Configuration.PreEnumerateTheories, StringComparison.OrdinalIgnoreCase) && property.Value.Type == JTokenType.Boolean)
                            result.PreEnumerateTheories = (bool)property.Value;
                    }
                }
                catch { }
            }

            return result;
        }

        static class Configuration
        {
            public const string DiagnosticMessages = "diagnosticMessages";
            public const string MaxParallelThreads = "maxParallelThreads";
            public const string MethodDisplay = "methodDisplay";
            public const string ParallelizeAssembly = "parallelizeAssembly";
            public const string ParallelizeTestCollections = "parallelizeTestCollections";
            public const string PreEnumerateTheories = "preEnumerateTheories";
        }
    }
}
