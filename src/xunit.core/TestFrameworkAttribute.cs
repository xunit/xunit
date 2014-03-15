using System;

namespace Xunit
{
    /// <summary>
    /// Used to decorate an assembly to allow the use a custom <see cref="T:Xunit.Sdk.ITestFramework"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class TestFrameworkAttribute : Attribute
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
