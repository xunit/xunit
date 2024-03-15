using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace Xunit
{
    /// <summary>
    /// This class is used to read configuration information for a test assembly.
    /// </summary>
    public static class ConfigReader
    {
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
            var result = ConfigReader_Json.Load(assemblyFileName, configFileName, warnings);
            if (result != null)
                return result;

#if NETFRAMEWORK
            result = ConfigReader_Configuration.Load(assemblyFileName, configFileName, warnings);
            if (result != null)
                return result;
#endif

            if (configFileName != null && warnings != null && warnings.Count == 0)
                warnings.Add(string.Format(CultureInfo.CurrentCulture, "Couldn't load config file '{0}': unknown file type", configFileName));

            return new TestAssemblyConfiguration();
        }

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
        public static TestAssemblyConfiguration Load(Stream configStream, List<string> warnings = null)
        {
            return ConfigReader_Json.Load(configStream, warnings);
        }
    }
}
