using System;
using System.Collections.Generic;

namespace Xunit
{
    /// <summary>
    /// Represents an assembly in an <see cref="XunitProject"/>.
    /// </summary>
    public class XunitProjectAssembly
    {
        TestAssemblyConfiguration configuration;

        /// <summary>
        /// Gets or sets the assembly filename.
        /// </summary>
        public string AssemblyFilename { get; set; }

        /// <summary>
        /// Gets or sets the config filename.
        /// </summary>
        public string ConfigFilename { get; set; }

        /// <summary>
        /// Gets the configuration values read from the test assembly configuration file.
        /// </summary>
        public TestAssemblyConfiguration Configuration
        {
            get
            {
                if (configuration == null)
                    configuration = ConfigReader.Load(AssemblyFilename, ConfigFilename, ConfigWarnings);

                return configuration;
            }
        }

        /// <summary>
        /// Gets the list of warnings that resulting from reading the configuration.
        /// </summary>
        public List<string> ConfigWarnings { get; } = new();

        /// <summary>
        /// Gets or sets a value indicating whether to shadow copy the assembly
        /// when running the tests.
        /// </summary>
        [Obsolete("Please use Configuration.ShadowCopyOrDefault (get) or Configuration.ShadowCopy (set) instead")]
        public bool ShadowCopy
        {
            get { return Configuration.ShadowCopyOrDefault; }
            set { Configuration.ShadowCopy = value; }
        }
    }
}
