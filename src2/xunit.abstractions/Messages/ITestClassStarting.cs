namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test class is about to begin running.
    /// </summary>
    public interface ITestClassStarting : ITestMessage
    {
        /// <summary>
        /// The fully-qualified name of the test class.
        /// </summary>
        string ClassName { get; }
    }
}