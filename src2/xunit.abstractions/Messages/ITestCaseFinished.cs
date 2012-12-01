namespace Xunit.Abstractions
{
    public interface ITestCaseFinished : IFinishedMessage
    {
        ITestCase TestCase { get; }
    }
}
