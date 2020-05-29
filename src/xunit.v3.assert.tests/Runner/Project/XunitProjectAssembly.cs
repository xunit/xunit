using System.Reflection;

namespace Xunit
{
    /// <summary>
    /// Represents an assembly in an <see cref="XunitProject"/>.
    /// </summary>
    public class XunitProjectAssembly
    {
        TestAssemblyConfiguration configuration = null;

        /// <summary>
        /// Gets or sets the assembly that will be tested.
        /// </summary>
        public Assembly Assembly { get; set; }

        /// <inheritdoc />
        public string AssemblyFilename => Assembly.GetLocalCodeBase();

        /// <inheritdoc />
        public string ConfigFilename { get; set; }

        /// <inheritdoc />
        public TestAssemblyConfiguration Configuration
        {
            get
            {
                if (configuration == null)
                    configuration = new TestAssemblyConfiguration();
                // configuration = ConfigReader.Load(AssemblyFilename, ConfigFilename);

                return configuration;
            }
        }
    }
}
