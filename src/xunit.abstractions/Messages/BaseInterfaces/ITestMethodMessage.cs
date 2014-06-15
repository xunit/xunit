namespace Xunit.Abstractions
{
    /// <summary>
    /// Base message interface for all messages related to test methods.
    /// </summary>
    public interface ITestMethodMessage : ITestClassMessage
    {
        /// <summary>
        /// The name of the test method.
        /// </summary>
        string MethodName { get; }
    }
}
