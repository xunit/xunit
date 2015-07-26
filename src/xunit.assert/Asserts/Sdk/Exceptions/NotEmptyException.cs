namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a collection is unexpectedly empty.
    /// </summary>
    public class NotEmptyException : XunitException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NotEmptyException"/> class.
        /// </summary>
        public NotEmptyException()
            : base("Assert.NotEmpty() Failure")
        { }
    }
}