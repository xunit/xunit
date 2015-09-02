using System;
using System.IO;

#if PLATFORM_DOTNET
using System.Reflection;
#endif

namespace Xunit
{
    /// <summary>
    /// This class is used to read configuration information for a test assembly.
    /// </summary>
    public static class ConfigReader_Json
    {
        /// <summary>
        /// Loads the test assembly configuration for the given test assembly from a JSON stream. Caller is responsible for opening the stream.
        /// </summary>
        /// <param name="configStream">Stream containing config for an assembly</param>
        /// <returns>The test assembly configuration.</returns>
        public static TestAssemblyConfiguration Load(Stream configStream)
        {
            var result = new TestAssemblyConfiguration();

            try
            {
                using (var reader = new StreamReader(configStream))
                {
                    var config = JsonDeserializer.Deserialize(reader) as JsonObject;

                    foreach (var propertyName in config.Keys)
                    {
                        var propertyValue = config.Value(propertyName);
                        var booleanValue = propertyValue as JsonBoolean;

                        if (booleanValue != null)
                        {
                            if (string.Equals(propertyName, Configuration.DiagnosticMessages, StringComparison.OrdinalIgnoreCase))
                                result.DiagnosticMessages = booleanValue;
                            if (string.Equals(propertyName, Configuration.ParallelizeAssembly, StringComparison.OrdinalIgnoreCase))
                                result.ParallelizeAssembly = booleanValue;
                            if (string.Equals(propertyName, Configuration.ParallelizeTestCollections, StringComparison.OrdinalIgnoreCase))
                                result.ParallelizeTestCollections = booleanValue;
                            if (string.Equals(propertyName, Configuration.PreEnumerateTheories, StringComparison.OrdinalIgnoreCase))
                                result.PreEnumerateTheories = booleanValue;
                        }
                        else if (string.Equals(propertyName, Configuration.MaxParallelThreads, StringComparison.OrdinalIgnoreCase))
                        {
                            var numberValue = propertyValue as JsonNumber;
                            if (numberValue != null)
                            {
                                int maxParallelThreads;
                                if (int.TryParse(numberValue.Raw, out maxParallelThreads) && maxParallelThreads > 0)
                                    result.MaxParallelThreads = maxParallelThreads;
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
            catch { }

            return result;
        }

        /// <summary>
        /// Loads the test assembly configuration for the given test assembly.
        /// </summary>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <returns>The test assembly configuration.</returns>
        public static TestAssemblyConfiguration Load(string assemblyFileName, string configFileName = null)
        {
            if (configFileName == null)
                configFileName = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.runner.json");

            if (configFileName.EndsWith(".json", StringComparison.Ordinal))
            {
                try
                {
                    using (var stream = File_OpenRead(configFileName))
                        return Load(stream);
                }
                catch { }
            }

            return null;
        }

#if PLATFORM_DOTNET
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
            public const string MaxParallelThreads = "maxParallelThreads";
            public const string MethodDisplay = "methodDisplay";
            public const string ParallelizeAssembly = "parallelizeAssembly";
            public const string ParallelizeTestCollections = "parallelizeTestCollections";
            public const string PreEnumerateTheories = "preEnumerateTheories";
        }
    }
}
