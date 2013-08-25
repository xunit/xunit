namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test method has finished executing (meaning, all
    /// the test cases that derived from the test method have finished).
    /// </summary>
    public interface ITestMethodFinished : ITestCollectionMessage
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