using System;

namespace Xunit.Abstractions
{
    public interface IErrorMessage : ITestMessage
    {
        Exception Error { get; }
    }
}
