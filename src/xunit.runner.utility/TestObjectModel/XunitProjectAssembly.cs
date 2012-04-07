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
            Output = new Dictionary<string, string>();
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
        /// <remarks>
        /// The xUnit.net GUI runner does not support this field.
        /// </remarks>
        public bool ShadowCopy { get; set; }

        /// <summary>
        /// Gets or sets the output filenames. The dictionary key is the type
        /// of the file to be output; the dictionary value is the filename to
        /// write the output to.
        /// </summary>
        /// <remarks>
        /// The xUnit.net GUI runner does not support this field. The MSBuild
        /// runner only supports output of type 'xml', 'html', and 'nunit'.
        /// </remarks>
        public Dictionary<string, string> Output { get; set; }
    }
}