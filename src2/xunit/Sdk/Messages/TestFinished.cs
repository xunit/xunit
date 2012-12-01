using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestFinished : ITestFinished
    {
        public ITestCase TestCase { get; set; }
        public string DisplayName { get; set; }
    }
}
