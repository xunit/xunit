namespace Xunit.Abstractions
{
    /// <summary>
    /// This message indicates that an instance of a test class has just been constructed.
    /// Instance (non-static) methods of tests get a new instance of the test class for each
    /// individual test execution; static methods do not get an instance of the test class.
    /// </summary>
    public interface ITestClassConstructionFinished : ITestMessage, IExecutionMessage
    {
    }
}