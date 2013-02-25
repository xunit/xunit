using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestFailed : TestResultMessage, ITestFailed
    {
        public Exception Exception { get; set; }
    }
}
