namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test method is about to begin executing.
    /// </summary>
    public interface ITestMethodStarting : ITestMessage
    {
        /// <summary>
        /// The fully-qualified name of the test class.
        /// </summary>
        string ClassName { get; }

        /// <summary>
        /// The name of the test method.
        /// </summary>
        string MethodName { get; }
    }
}