using System.IO;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="IResultXmlTransform"/> which writes the
    /// XML to a file without any transformation applied.
    /// </summary>
    public class NullTransformer : IResultXmlTransform
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullTransformer"/> class.
        /// </summary>
        /// <param name="fileName">The output filename.</param>
        public NullTransformer(string fileName)
        {
            OutputFilename = fileName;
        }

        /// <inheritdoc/>
        public string OutputFilename { get; private set; }

        /// <inheritdoc/>
        public void Transform(string xml)
        {
            File.WriteAllText(OutputFilename, xml);
        }
    }
}
