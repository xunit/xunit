using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestAssemblyStarting : ITestAssemblyStarting
    {
        public IAssemblyInfo Assembly { get; set; }
    }
}
