namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when two values are unexpectedly equal.
    /// </summary>
    public class NotEqualException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NotEqualException"/> class.
        /// </summary>
        public NotEqualException(string expected, string actual)
            : base("Not " + expected, actual, "Assert.NotEqual() Failure")
        { }
    }
}