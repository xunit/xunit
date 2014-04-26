namespace Xunit.Sdk
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Exception thrown when a set is not a proper subset of another set.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class ProperSubsetException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ProperSubsetException"/> class.
        /// </summary>
        public ProperSubsetException()
            : base("Assert.ProperSubset() Failure") { }
    }
}