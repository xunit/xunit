using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestClassDisposeFinished : LongLivedMarshalByRefObject, ITestClassDisposeFinished
    {
        public ITestCase TestCase { get; set; }
        public string TestDisplayName { get; set; }
    }
}
