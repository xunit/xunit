using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestCaseStarting : ITestCaseStarting
    {
        public ITestCase TestCase { get; set; }
    }
}
