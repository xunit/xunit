using System;
using System.Configuration;

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
            if (configFileName == null)
                configFileName = assemblyFileName + ".config";

            var result = new TestAssemblyConfiguration();

            try
            {
                var map = new ExeConfigurationFileMap { ExeConfigFilename = configFileName };
                var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                if (config != null && config.AppSettings != null)
                {
                    var settings = config.AppSettings.Settings;

                    result.DiagnosticMessages = GetBoolean(settings, TestOptionsNames.Configuration.DiagnosticMessages) ?? result.DiagnosticMessages;
                    result.MaxParallelThreads = GetInt(settings, TestOptionsNames.Configuration.MaxParallelThreads) ?? result.MaxParallelThreads;
                    result.MethodDisplay = GetEnum<TestMethodDisplay>(settings, TestOptionsNames.Configuration.MethodDisplay) ?? result.MethodDisplay;
                    result.ParallelizeAssembly = GetBoolean(settings, TestOptionsNames.Configuration.ParallelizeAssembly) ?? result.ParallelizeAssembly;
                    result.ParallelizeTestCollections = GetBoolean(settings, TestOptionsNames.Configuration.ParallelizeTestCollections) ?? result.ParallelizeTestCollections;
                    result.PreEnumerateTheories = GetBoolean(settings, TestOptionsNames.Configuration.PreEnumerateTheories) ?? result.PreEnumerateTheories;
                }
            }
            catch (ConfigurationErrorsException) { }

            return result;
        }

        static bool? GetBoolean(KeyValueConfigurationCollection settings, string key)
        {
            return GetValue<bool?>(settings, key,
                value =>
                {
                    switch (value.ToLowerInvariant())
                    {
                        case "true": return true;
                        case "false": return false;
                        default: return null;
                    }
                });
        }

        static TValue? GetEnum<TValue>(KeyValueConfigurationCollection settings, string key)
            where TValue : struct
        {
            return GetValue<TValue?>(settings, key,
                value =>
                {
                    try { return (TValue)Enum.Parse(typeof(TValue), value, ignoreCase: true); }
                    catch { return null; }
                });
        }

        static int? GetInt(KeyValueConfigurationCollection settings, string key)
        {
            return GetValue<int?>(settings, key,
                ValueType =>
                {
                    int result;
                    if (Int32.TryParse(ValueType, out result))
                        return result;
                    return null;
                });
        }

        static T GetValue<T>(KeyValueConfigurationCollection settings, string key, Func<string, T> converter)
        {
            var setting = settings[key];
            if (setting == null)
                return default(T);

            return converter(setting.Value);
        }
    }
}
