using System;

namespace Xunit
{
    /// <summary>
    /// Used to decorate an assembly to allow the use of a custom <see cref="T:Xunit.Sdk.ITestCollectionOrderer"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = true, AllowMultiple = false)]
    public sealed class TestCollectionOrdererAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCollectionOrdererAttribute"/> class.
        /// </summary>
        /// <param name="ordererTypeName">The type name of the orderer class (that implements <see cref="T:Xunit.Sdk.ITestCollectionOrderer"/>).</param>
        /// <param name="ordererAssemblyName">The assembly that <paramref name="ordererTypeName"/> exists in.</param>
        public TestCollectionOrdererAttribute(string ordererTypeName, string ordererAssemblyName) { }
    }
}
