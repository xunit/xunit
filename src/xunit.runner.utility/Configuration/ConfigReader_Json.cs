using System;
using System.IO;

namespace Xunit
{
    /// <summary>
    /// This class is used to read configuration information for a test assembly.
    /// </summary>
    public static class ConfigReader_Json
    {
        // HACK: This is necssary to back-port support for dnx451, because .NET Core relies
        // upon the System.IO.FileSystem NuGet package, which is not compatible with net451.
#if !DOTNETCORE
        static ConfigReader_Json()
        {
            FileOpenRead = System.IO.File.OpenRead;
        }
#endif

        /// <summary>
        /// Represents the ability open a file for reading. Any .NET Core runners must set
        /// this value before tests will run correctly. This requirement will be removed
        /// once support for dnx451 is removed in favor of dnx46.
        /// </summary>
        public static Func<string, Stream> FileOpenRead { get; set; }

        /// <summary>
        /// Loads the test assembly configuration for the given test assembly.
        /// </summary>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <returns>The test assembly configuration.</returns>
        public static TestAssemblyConfiguration Load(string assemblyFileName, string configFileName = null)
        {
            if (FileOpenRead == null)
                throw new InvalidOperationException("Runner authors in .NET Core must set ConfigReader_Json.FileOpenRead.");

            if (configFileName == null)
                configFileName = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.runner.json");

            if (configFileName.EndsWith(".json", StringComparison.Ordinal))
            {
                try
                {
                    var result = new TestAssemblyConfiguration();

                    using (var stream = FileOpenRead(configFileName))
                    using (var reader = new StreamReader(stream))
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
                                if (string.Equals(propertyName, Configuration.UseAppDomain, StringComparison.OrdinalIgnoreCase))
                                    result.UseAppDomain = booleanValue;
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
                        }
                    }

                    return result;
                }
                catch { }
            }

            return null;
        }

        static class Configuration
        {
            public const string DiagnosticMessages = "diagnosticMessages";
            public const string MaxParallelThreads = "maxParallelThreads";
            public const string MethodDisplay = "methodDisplay";
            public const string ParallelizeAssembly = "parallelizeAssembly";
            public const string ParallelizeTestCollections = "parallelizeTestCollections";
            public const string PreEnumerateTheories = "preEnumerateTheories";
            public const string UseAppDomain = "useAppDomain";
        }
    }
}
