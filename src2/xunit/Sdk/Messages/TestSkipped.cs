using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestSkipped : TestResultMessage, ITestSkipped
    {
        public string Reason { get; set; }
    }
}
