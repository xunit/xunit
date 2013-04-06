namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that a test class has finished executing (meaning, all of the
    /// test cases in this test class have finished running).
    /// </summary>
    public interface ITestClassFinished : IFinishedMessage
    {
        /// <summary>
        /// The fully-qualified name of the test class.
        /// </summary>
        string ClassName { get; }
    }
}