using System;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Used to decorate an assembly to allow the use of a custom <see cref="T:Xunit.Sdk.ITestFramework"/>.
    /// </summary>
    [TestFrameworkDiscoverer("Xunit.Sdk.TestFrameworkTypeDiscoverer", "xunit.execution.{Platform}")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class TestFrameworkAttribute : Attribute, ITestFrameworkAttribute
    {
        /// <summary>
        /// Initializes an instance of <see cref="TestFrameworkAttribute"/>.
        /// </summary>
        /// <param name="typeName">The fully qualified type name of the test framework
        /// (f.e., 'Xunit.Sdk.XunitTestFramework')</param>
        /// <param name="assemblyName">The name of the assembly that the test framework type
        /// is located in, without file extension (f.e., 'xunit.execution')</param>
        public TestFrameworkAttribute(string typeName, string assemblyName) { }
    }
}
