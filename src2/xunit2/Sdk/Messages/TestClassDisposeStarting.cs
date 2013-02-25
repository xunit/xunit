using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestClassDisposeStarting : LongLivedMarshalByRefObject, ITestClassDisposeStarting
    {
        public ITestCase TestCase { get; set; }
        public string TestDisplayName { get; set; }
    }
}
