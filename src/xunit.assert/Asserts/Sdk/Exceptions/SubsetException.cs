namespace Xunit.Sdk
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Exception thrown when a set is not a subset of another set.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class SubsetException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SubsetException"/> class.
        /// </summary>
        public SubsetException()
            : base("Assert.Subset() Failure") { }
    }
}