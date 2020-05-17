namespace Xunit.Abstractions
{
    /// <summary>
    /// Represents source information about a test case.
    /// </summary>
    public interface ISourceInformation : IXunitSerializable
    {
        /// <summary>
        /// Gets or sets the source file name. A <c>null</c> value indicates that the
        /// source file name is not known.
        /// </summary>
        string FileName { get; set; }

        /// <summary>
        /// Gets or sets the source file line. A <c>null</c> value indicates that the
        /// source file line is not known.
        /// </summary>
        int? LineNumber { get; set; }
    }
}
