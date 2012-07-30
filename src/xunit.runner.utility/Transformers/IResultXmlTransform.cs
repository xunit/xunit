using System.Diagnostics.CodeAnalysis;

namespace Xunit
{
    /// <summary>
    /// Represents a transformation of the resulting assembly XML into some output format.
    /// </summary>
    public interface IResultXmlTransform
    {
        /// <summary>
        /// Gets the output filename, if known; returns null if the output isn't done to file.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Filename", Justification = "This would be a breaking change.")]
        string OutputFilename { get; }

        /// <summary>
        /// Transforms the given assembly XML into the destination format.
        /// </summary>
        /// <param name="xml">The assembly XML.</param>
        void Transform(string xml);
    }
}