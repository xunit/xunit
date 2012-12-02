using System;

namespace Xunit.Abstractions
{
    public interface ITestFailed : ITestResultMessage
    {
        Exception Exception { get; }
    }
}
