using System;

namespace Xunit.Abstractions
{
    public interface IFailedTestCaseResult : ITestCaseResult
    {
        Exception Exception { get; }
    }
}
