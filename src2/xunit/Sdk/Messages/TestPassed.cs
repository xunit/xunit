using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestPassed : ITestPassed
    {
        public ITestCase TestCase { get; set; }
        public string DisplayName { get; set; }
    }
}
