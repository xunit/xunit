namespace Xunit.Sdk
{
    /// <summary>
    /// Exception thrown when a value an implication is not satisfied.
    /// </summary>
    public class ImplyException : AssertActualExpectedException
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ImplyException"/> class.
        /// </summary>
        /// <param name="userMessage">The user message to be displayed, or null for the default message</param>
        public ImplyException(string userMessage)
            : base("Implies", "Does not imply", userMessage ?? "Assert.Imply() Failure")
        { }
    }
}