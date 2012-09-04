using System;

namespace Xunit.Sdk2
{
    public interface IFailedTestCaseResult : ITestCaseResult
    {
        Exception Exception { get; }
    }
}
