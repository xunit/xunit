using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestClassConstructionStarting : LongLivedMarshalByRefObject, ITestClassConstructionStarting
    {
        public ITestCase TestCase { get; set; }
        public string TestDisplayName { get; set; }
    }
}
