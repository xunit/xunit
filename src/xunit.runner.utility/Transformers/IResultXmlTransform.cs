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
        string OutputFilename { get; }

        /// <summary>
        /// Transforms the given assembly XML into the destination format.
        /// </summary>
        /// <param name="xml">The assembly XML.</param>
        void Transform(string xml);
    }
}