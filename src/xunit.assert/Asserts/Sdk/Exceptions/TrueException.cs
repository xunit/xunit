namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a value is unexpectedly false.
    /// </summary>
    public class TrueException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TrueException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be displayed, or null for the default message</param>
        /// <param name="value">The actual value</param>
        public TrueException(string userMessage, bool? value)
            : base("True", value == null ? "(null)" : value.ToString(), userMessage ?? "Assert.True() Failure")
        { }
    }
}