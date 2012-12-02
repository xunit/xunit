using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestCollectionStarting : LongLivedMarshalByRefObject, ITestCollectionStarting
    {
        public IAssemblyInfo Assembly { get; set; }
    }
}
