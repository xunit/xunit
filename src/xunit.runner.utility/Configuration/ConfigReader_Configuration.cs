#if NETFRAMEWORK

using System;
using System.Configuration;

namespace Xunit
{
    /// <summary>
    /// This class is used to read configuration information for a test assembly.
    /// </summary>
    public static class ConfigReader_Configuration
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

            if (configFileName.EndsWith(".config", StringComparison.Ordinal))
            {
                try
                {
                    var map = new ExeConfigurationFileMap { ExeConfigFilename = configFileName };
                    var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
                    if (config != null && config.AppSettings != null)
                    {
                        var result = new TestAssemblyConfiguration();
                        var settings = config.AppSettings.Settings;

                        result.AppDomain = GetEnum<AppDomainSupport>(settings, Configuration.AppDomain) ?? result.AppDomain;
                        result.DiagnosticMessages = GetBoolean(settings, Configuration.DiagnosticMessages) ?? result.DiagnosticMessages;
                        result.InternalDiagnosticMessages = GetBoolean(settings, Configuration.InternalDiagnosticMessages) ?? result.InternalDiagnosticMessages;
                        result.MaxParallelThreads = GetInt(settings, Configuration.MaxParallelThreads) ?? result.MaxParallelThreads;
                        result.MethodDisplay = GetEnum<TestMethodDisplay>(settings, Configuration.MethodDisplay) ?? result.MethodDisplay;
                        result.MethodDisplayOptions = GetEnum<TestMethodDisplayOptions>(settings, Configuration.MethodDisplayOptions) ?? result.MethodDisplayOptions;
                        result.ParallelizeAssembly = GetBoolean(settings, Configuration.ParallelizeAssembly) ?? result.ParallelizeAssembly;
                        result.ParallelizeTestCollections = GetBoolean(settings, Configuration.ParallelizeTestCollections) ?? result.ParallelizeTestCollections;
                        result.PreEnumerateTheories = GetBoolean(settings, Configuration.PreEnumerateTheories) ?? result.PreEnumerateTheories;
                        result.ShadowCopy = GetBoolean(settings, Configuration.ShadowCopy) ?? result.ShadowCopy;
                        result.StopOnFail = GetBoolean(settings, Configuration.StopOnFail) ?? result.StopOnFail;
                        result.LongRunningTestSeconds = GetInt(settings, Configuration.LongRunningTestSeconds) ?? result.LongRunningTestSeconds;

                        return result;
                    }
                }
                catch { }
            }

            return null;
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
                    try { return (TValue)Enum.Parse(typeof(TValue), value, true); }
                    catch { return null; }
                });
        }

        static int? GetInt(KeyValueConfigurationCollection settings, string key)
        {
            return GetValue<int?>(settings, key,
                ValueType =>
                {
                    int result;
                    if (int.TryParse(ValueType, out result))
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

        static class Configuration
        {
            public const string AppDomain = "xunit.appDomain";
            public const string DiagnosticMessages = "xunit.diagnosticMessages";
            public const string InternalDiagnosticMessages = "xunit.internalDiagnosticMessages";
            public const string MaxParallelThreads = "xunit.maxParallelThreads";
            public const string MethodDisplay = "xunit.methodDisplay";
            public const string MethodDisplayOptions = "xunit.methodDisplayOptions";
            public const string ParallelizeAssembly = "xunit.parallelizeAssembly";
            public const string ParallelizeTestCollections = "xunit.parallelizeTestCollections";
            public const string PreEnumerateTheories = "xunit.preEnumerateTheories";
            public const string ShadowCopy = "xunit.shadowCopy";
            public const string StopOnFail = "xunit.stopOnFail";
            public const string LongRunningTestSeconds = "xunit.longRunningTestSeconds";
        }
    }
}

#endif
