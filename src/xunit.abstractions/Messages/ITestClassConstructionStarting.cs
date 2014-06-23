namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that an instance of a test class is about to be constructed.
    /// Instance (non-static) methods of tests get a new instance of the test class for each
    /// individual test execution; static methods do not get an instance of the test class.
    /// </summary>
    public interface ITestClassConstructionStarting : ITestMessage, IExecutionMessage
    {
    }
}