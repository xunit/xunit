using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestClassStarting : ITestClassStarting
    {
        public IAssemblyInfo Assembly { get; set; }
        public string ClassName { get; set; }
    }
}
