using System;

namespace Xunit.Sdk
{
    /// <summary>
    /// An attribute used to decorate classes which derive from <see cref="FactAttribute"/>,
    /// to indicate how test cases should be discovered.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class TestCaseDiscovererAttribute : Attribute
    {
        /// <summary>
        /// Initializes an instance of the <see cref="TestCaseDiscovererAttribute"/> class.
        /// </summary>
        /// <param name="typeName">The fully qualified type name of the discoverer
        /// (f.e., 'Xunit.Sdk.FactDiscoverer')</param>
        /// <param name="assemblyName">The name of the assembly that the discoverer type
        /// is located in, without file extension (f.e., 'xunit2')</param>
        public TestCaseDiscovererAttribute(string typeName, string assemblyName) { }
    }
}