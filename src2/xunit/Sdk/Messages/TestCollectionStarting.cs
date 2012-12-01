using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestCollectionStarting : ITestCollectionStarting
    {
        public IAssemblyInfo Assembly { get; set; }
    }
}
