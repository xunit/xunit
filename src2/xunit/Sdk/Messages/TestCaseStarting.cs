using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestCaseStarting : LongLivedMarshalByRefObject, ITestCaseStarting
    {
        public ITestCase TestCase { get; set; }
    }
}
