using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;

#if NETSTANDARD1_1
using System.Reflection;
#endif

namespace Xunit
{
    /// <summary>
    /// This class is used to read configuration information for a test assembly.
    /// </summary>
    public static class ConfigReader_Json
    {
        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static TestAssemblyConfiguration Load(Stream configStream) =>
            Load(configStream, null);

        /// <summary>
        /// Loads the test assembly configuration for the given test assembly from a JSON stream. Caller is responsible for opening the stream.
        /// </summary>
        /// <param name="configStream">Stream containing config for an assembly</param>
        /// <param name="warnings">A container to receive loading warnings, if desired.</param>
        /// <returns>The test assembly configuration.</returns>
        public static TestAssemblyConfiguration Load(Stream configStream, List<string> warnings = null) =>
            LoadConfiguration(configStream, null, warnings);

        /// <summary/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static TestAssemblyConfiguration Load(string assemblyFileName, string configFileName = null) =>
            Load(assemblyFileName, configFileName, null);

        /// <summary>
        /// Loads the test assembly configuration for the given test assembly.
        /// </summary>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <param name="warnings">A container to receive loading warnings, if desired.</param>
        /// <returns>The test assembly configuration.</returns>
        public static TestAssemblyConfiguration Load(string assemblyFileName, string configFileName = null, List<string> warnings = null)
        {
            // If they provide a configuration file, we only read that, success or failure
            if (configFileName != null)
            {
                if (!configFileName.EndsWith(".json", StringComparison.Ordinal))
                    return null;

#if !NETSTANDARD1_1
                if (!File.Exists(configFileName))
                {
                    warnings?.Add(string.Format(CultureInfo.CurrentCulture, "Couldn't load config file '{0}': file not found", configFileName));
                    return null;
                }
#endif

                return LoadFile(configFileName, warnings);
            }

            var assemblyName = Path.GetFileNameWithoutExtension(assemblyFileName);
            var directoryName = Path.GetDirectoryName(assemblyFileName);

            return LoadFile(Path.Combine(directoryName, string.Format(CultureInfo.InvariantCulture, "{0}.xunit.runner.json", assemblyName)), warnings)
                ?? LoadFile(Path.Combine(directoryName, "xunit.runner.json"), warnings);
        }

        static TestAssemblyConfiguration LoadConfiguration(Stream configStream, string configFileName, List<string> warnings)
        {
            Guard.ArgumentNotNull(nameof(configStream), configStream);

            string ConfigDescription() =>
                configFileName == null
                    ? "configuration"
                    : string.Format(CultureInfo.CurrentCulture, "config file '{0}'", configFileName);

            var result = new TestAssemblyConfiguration();

            try
            {
                using (var reader = new StreamReader(configStream))
                {
                    var config = JsonDeserializer.Deserialize(reader) as JsonObject;

                    if (config == null)
                    {
                        warnings?.Add(string.Format(CultureInfo.CurrentCulture, "Couldn't parse {0}: the root must be a JSON object", ConfigDescription()));
                        return null;
                    }

                    foreach (var propertyName in config.Keys)
                    {
                        var propertyValue = config.Value(propertyName);
                        var booleanValue = propertyValue as JsonBoolean;

                        if (booleanValue != null)
                        {
                            if (string.Equals(propertyName, Configuration.DiagnosticMessages, StringComparison.OrdinalIgnoreCase))
                                result.DiagnosticMessages = booleanValue;
                            if (string.Equals(propertyName, Configuration.FailSkips, StringComparison.OrdinalIgnoreCase))
                                result.FailSkips = booleanValue;
                            if (string.Equals(propertyName, Configuration.InternalDiagnosticMessages, StringComparison.OrdinalIgnoreCase))
                                result.InternalDiagnosticMessages = booleanValue;
                            if (string.Equals(propertyName, Configuration.ParallelizeAssembly, StringComparison.OrdinalIgnoreCase))
                                result.ParallelizeAssembly = booleanValue;
                            if (string.Equals(propertyName, Configuration.ParallelizeTestCollections, StringComparison.OrdinalIgnoreCase))
                                result.ParallelizeTestCollections = booleanValue;
                            if (string.Equals(propertyName, Configuration.PreEnumerateTheories, StringComparison.OrdinalIgnoreCase))
                                result.PreEnumerateTheories = booleanValue;
                            if (string.Equals(propertyName, Configuration.ShadowCopy, StringComparison.OrdinalIgnoreCase))
                                result.ShadowCopy = booleanValue;
                            if (string.Equals(propertyName, Configuration.ShowLiveOutput, StringComparison.OrdinalIgnoreCase))
                                result.ShowLiveOutput = booleanValue;
                            if (string.Equals(propertyName, Configuration.StopOnFail, StringComparison.OrdinalIgnoreCase))
                                result.StopOnFail = booleanValue;
                        }
                        else if (string.Equals(propertyName, Configuration.MaxParallelThreads, StringComparison.OrdinalIgnoreCase))
                        {
                            if (propertyValue is JsonNumber numberValue)
                            {
                                int maxParallelThreads;
                                if (int.TryParse(numberValue.Raw, out maxParallelThreads) && maxParallelThreads >= -1)
                                    result.MaxParallelThreads = maxParallelThreads;
                            }
                            else if (propertyValue is JsonString stringValue)
                            {
                                if (string.Equals("default", stringValue, StringComparison.OrdinalIgnoreCase))
                                    result.MaxParallelThreads = 0;
                                else if (string.Equals("unlimited", stringValue, StringComparison.OrdinalIgnoreCase))
                                    result.MaxParallelThreads = -1;
                                else
                                {
                                    var match = ConfigUtility.MultiplierStyleMaxParallelThreadsRegex.Match(stringValue);
                                    // Use invariant format and convert ',' to '.' so we can always support both formats, regardless of locale
                                    // If we stick to locale-only parsing, we could break people when moving from one locale to another (for example,
                                    // from people running tests on their desktop in a comma locale vs. running them in CI with a decimal locale).
                                    if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var maxThreadMultiplier))
                                        result.MaxParallelThreads = (int)(maxThreadMultiplier * Environment.ProcessorCount);
                                }
                            }
                        }
                        else if (string.Equals(propertyName, Configuration.LongRunningTestSeconds, StringComparison.OrdinalIgnoreCase))
                        {
                            var numberValue = propertyValue as JsonNumber;
                            if (numberValue != null)
                            {
                                int seconds;
                                if (int.TryParse(numberValue.Raw, out seconds) && seconds > 0)
                                    result.LongRunningTestSeconds = seconds;
                            }
                        }
                        else if (string.Equals(propertyName, Configuration.MethodDisplay, StringComparison.OrdinalIgnoreCase))
                        {
                            var stringValue = propertyValue as JsonString;
                            if (stringValue != null)
                            {
                                try
                                {
                                    var methodDisplay = Enum.Parse(typeof(TestMethodDisplay), stringValue, true);
                                    result.MethodDisplay = (TestMethodDisplay)methodDisplay;
                                }
                                catch { }
                            }
                        }
                        else if (string.Equals(propertyName, Configuration.MethodDisplayOptions, StringComparison.OrdinalIgnoreCase))
                        {
                            var stringValue = propertyValue as JsonString;
                            if (stringValue != null)
                            {
                                try
                                {
                                    var methodDisplayOptions = Enum.Parse(typeof(TestMethodDisplayOptions), stringValue, true);
                                    result.MethodDisplayOptions = (TestMethodDisplayOptions)methodDisplayOptions;
                                }
                                catch { }
                            }
                        }
                        else if (string.Equals(propertyName, Configuration.ParallelAlgorithm, StringComparison.OrdinalIgnoreCase))
                        {
                            var stringValue = propertyValue as JsonString;
                            if (stringValue != null)
                            {
                                try
                                {
                                    var parallelAlgorithm = Enum.Parse(typeof(ParallelAlgorithm), stringValue, true);
                                    result.ParallelAlgorithm = (ParallelAlgorithm)parallelAlgorithm;
                                }
                                catch { }
                            }
                        }
                        else if (string.Equals(propertyName, Configuration.AppDomain, StringComparison.OrdinalIgnoreCase))
                        {
                            var stringValue = propertyValue as JsonString;
                            if (stringValue != null)
                            {
                                try
                                {
                                    var appDomain = Enum.Parse(typeof(AppDomainSupport), stringValue, true);
                                    result.AppDomain = (AppDomainSupport)appDomain;
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                warnings?.Add(string.Format(CultureInfo.CurrentCulture, "Exception loading {0}: {1}", ConfigDescription(), ex.Message));
            }

            return result;
        }

        static TestAssemblyConfiguration LoadFile(string configFileName, List<string> warnings = null)
        {
#if !NETSTANDARD1_1
            if (!File.Exists(configFileName))
                return null;
#endif
            try
            {
                using (var stream = File_OpenRead(configFileName))
                    return LoadConfiguration(stream, configFileName, warnings);
            }
            catch { }

            return null;
        }

#if NETSTANDARD1_1
        static Lazy<MethodInfo> fileOpenReadMethod = new Lazy<MethodInfo>(GetFileOpenReadMethod);

        static MethodInfo GetFileOpenReadMethod()
        {
            var fileType = Type.GetType("System.IO.File");
            if (fileType == null)
                throw new InvalidOperationException("Could not load type: System.IO.File");

            var fileOpenReadMethod = fileType.GetRuntimeMethod("OpenRead", new[] { typeof(string) });
            if (fileOpenReadMethod == null)
                throw new InvalidOperationException("Could not find method: System.IO.File.OpenRead");

            return fileOpenReadMethod;
        }

        static Stream File_OpenRead(string path)
        {
            return (Stream)fileOpenReadMethod.Value.Invoke(null, new object[] { path });
        }
#else
        static Stream File_OpenRead(string path)
        {
            return File.OpenRead(path);
        }
#endif

        static class Configuration
        {
            public const string AppDomain = "appDomain";
            public const string DiagnosticMessages = "diagnosticMessages";
            public const string FailSkips = "failSkips";
            public const string InternalDiagnosticMessages = "internalDiagnosticMessages";
            public const string LongRunningTestSeconds = "longRunningTestSeconds";
            public const string MaxParallelThreads = "maxParallelThreads";
            public const string MethodDisplay = "methodDisplay";
            public const string MethodDisplayOptions = "methodDisplayOptions";
            public const string ParallelAlgorithm = "parallelAlgorithm";
            public const string ParallelizeAssembly = "parallelizeAssembly";
            public const string ParallelizeTestCollections = "parallelizeTestCollections";
            public const string PreEnumerateTheories = "preEnumerateTheories";
            public const string ShadowCopy = "shadowCopy";
            public const string ShowLiveOutput = "showLiveOutput";
            public const string StopOnFail = "stopOnFail";
        }
    }
}
