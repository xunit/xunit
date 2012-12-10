using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestClassConstructionFinished : LongLivedMarshalByRefObject, ITestClassConstructionFinished
    {
        public ITestCase TestCase { get; set; }
        public string TestDisplayName { get; set; }
    }
}
