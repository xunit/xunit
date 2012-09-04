using Xunit.Abstractions;

namespace Xunit.Sdk2
{
    public interface ITestCaseResult
    {
        double ExecutionTime { get; }

        string Output { get; }

        ITestCase TestCase { get; }
    }
}
