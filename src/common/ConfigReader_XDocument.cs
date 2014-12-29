using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Xunit
{
    /// <summary>
    /// This class is used to read configuration information for a test assembly.
    /// </summary>
#if UNIT_TEST
    public static class ConfigReader_XDocument
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
                configFileName = assemblyFileName + ".config";

            var result = new TestAssemblyConfiguration();

            try
            {
                using (var stream = File.OpenRead(configFileName))
                {
                    var document = XDocument.Load(stream);
                    var appSettings = document.Root.Element("appSettings");

                    if (appSettings != null)
                    {
                        foreach (var add in appSettings.Elements("add"))
                        {
                            var key = add.Attribute("key");
                            var value = add.Attribute("value");

                            if (key != null && value != null)
                            {
                                if (key.Value.Equals(TestOptionsNames.Configuration.DiagnosticMessages, StringComparison.OrdinalIgnoreCase))
                                    result.DiagnosticMessages = GetBoolean(value.Value, result.DiagnosticMessages);
                                else if (key.Value.Equals(TestOptionsNames.Configuration.MaxParallelThreads, StringComparison.OrdinalIgnoreCase))
                                    result.MaxParallelThreads = GetInt(value.Value, result.MaxParallelThreads);
                                else if (key.Value.Equals(TestOptionsNames.Configuration.MethodDisplay, StringComparison.OrdinalIgnoreCase))
                                    result.MethodDisplay = GetEnum(value.Value, result.MethodDisplay);
                                else if (key.Value.Equals(TestOptionsNames.Configuration.ParallelizeAssembly, StringComparison.OrdinalIgnoreCase))
                                    result.ParallelizeAssembly = GetBoolean(value.Value, result.ParallelizeAssembly);
                                else if (key.Value.Equals(TestOptionsNames.Configuration.ParallelizeTestCollections, StringComparison.OrdinalIgnoreCase))
                                    result.ParallelizeTestCollections = GetBoolean(value.Value, result.ParallelizeTestCollections);
                                else if (key.Value.Equals(TestOptionsNames.Configuration.PreEnumerateTheories, StringComparison.OrdinalIgnoreCase))
                                    result.PreEnumerateTheories = GetBoolean(value.Value, result.PreEnumerateTheories);
                            }
                        }
                    }
                }
            }
            catch { }

            return result;
        }

        static bool GetBoolean(string value, bool defaultValue)
        {
            switch (value.ToLowerInvariant())
            {
                case "true": return true;
                case "false": return false;
                default: return defaultValue;
            }
        }

        static TValue GetEnum<TValue>(string value, TValue defaultValue)
            where TValue : struct
        {
            TValue result;
            if (Enum.TryParse<TValue>(value, ignoreCase: true, result: out result))
                return result;
            return defaultValue;
        }

        static int GetInt(string value, int defaultValue)
        {
            int result;
            if (Int32.TryParse(value, out result))
                return result;
            return defaultValue;
        }
    }
}
