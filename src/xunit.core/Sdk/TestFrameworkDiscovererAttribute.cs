using System;

namespace Xunit.Sdk
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class TestFrameworkDiscovererAttribute : Attribute
    {
        /// <summary>
        /// Initializes an instance of <see cref="TestFrameworkDiscovererAttribute"/>.
        /// </summary>
        /// <param name="typeName">The fully qualified type name of the discoverer
        /// (f.e., 'Xunit.Sdk.DataDiscoverer')</param>
        /// <param name="assemblyName">The name of the assembly that the discoverer type
        /// is located in, without file extension (f.e., 'xunit.execution')</param>
        public TestFrameworkDiscovererAttribute(string typeName, string assemblyName) { }
    }
}
