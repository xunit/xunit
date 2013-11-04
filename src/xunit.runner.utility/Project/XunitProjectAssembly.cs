using System.Collections.Generic;

namespace Xunit
{
    /// <summary>
    /// Represents an assembly in an <see cref="XunitProject"/>.
    /// </summary>
    public class XunitProjectAssembly
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XunitProjectAssembly"/> class.
        /// </summary>
        public XunitProjectAssembly()
        {
            ShadowCopy = true;
        }

        /// <summary>
        /// Gets or sets the assembly filename.
        /// </summary>
        public string AssemblyFilename { get; set; }

        /// <summary>
        /// Gets or sets the config filename.
        /// </summary>
        public string ConfigFilename { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to shadow copy the assembly
        /// when running the tests.
        /// </summary>
        public bool ShadowCopy { get; set; }
    }
}
