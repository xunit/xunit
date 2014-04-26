namespace Xunit.Sdk
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Exception thrown when a set is not a superset of another set.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
    public class SupersetException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SupersetException"/> class.
        /// </summary>
        public SupersetException()
            : base("Assert.Superset() Failure") { }
    }
}