using System;

namespace Xunit.Sdk
{
    public interface IFailedTestCaseResult : ITestCaseResult
    {
        Exception Exception { get; }
    }
}
