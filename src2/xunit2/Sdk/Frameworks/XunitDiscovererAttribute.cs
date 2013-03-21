using System;

namespace Xunit.Sdk
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class XunitDiscovererAttribute : Attribute
    {
        /// <summary>
        /// Initializes an instance of <see cref="XunitDiscovererAttribute"/>.
        /// </summary>
        /// <param name="typeName">The fully qualified type name of the discoverer
        /// (f.e., 'Xunit.Sdk.FactDiscoverer')</param>
        /// <param name="assemblyName">The name of the assembly that the discoverer type
        /// is located in, without file extension (f.e., 'xunit2')</param>
        public XunitDiscovererAttribute(string typeName, string assemblyName) { }
    }
}