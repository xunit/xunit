using System;

namespace Xunit.Sdk
{
    /// <summary>
    /// An attribute used to decorate classes which derive from <see cref="DataAttribute"/>,
    /// to indicate how data elements should be discovered.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DataDiscovererAttribute : Attribute
    {
        /// <summary>
        /// Initializes an instance of <see cref="DataDiscovererAttribute"/>.
        /// </summary>
        /// <param name="typeName">The fully qualified type name of the discoverer
        /// (f.e., 'Xunit.Sdk.DataDiscoverer')</param>
        /// <param name="assemblyName">The name of the assembly that the discoverer type
        /// is located in, without file extension (f.e., 'xunit.execution')</param>
        public DataDiscovererAttribute(string typeName, string assemblyName) { }
    }
}
