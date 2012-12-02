using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestAssemblyStarting : LongLivedMarshalByRefObject, ITestAssemblyStarting
    {
        public IAssemblyInfo Assembly { get; set; }
    }
}
