namespace Xunit.Sdk
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Exception thrown when a set is not a proper superset of another set.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class ProperSupersetException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ProperSupersetException"/> class.
        /// </summary>
        public ProperSupersetException()
            : base("Assert.ProperSuperset() Failure") { }
    }
}