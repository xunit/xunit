using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public interface ITestCaseResult
    {
        double ExecutionTime { get; }

        string Output { get; }

        ITestCase TestCase { get; }
    }
}
