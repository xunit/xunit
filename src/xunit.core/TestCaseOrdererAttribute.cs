using System;

namespace Xunit
{
    /// <summary>
    /// Used to decorate an assembly, test collection, or test class to allow
    /// the use of a custom <see cref="T:Xunit.Sdk.ITestCaseOrderer"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class TestCaseOrdererAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCaseOrdererAttribute"/> class.
        /// </summary>
        /// <param name="ordererTypeName">The type name of the orderer class (that implements <see cref="T:Xunit.Sdk.ITestCaseOrderer"/>).</param>
        /// <param name="ordererAssemblyName">The assembly that <paramref name="ordererTypeName"/> exists in.</param>
        public TestCaseOrdererAttribute(string ordererTypeName, string ordererAssemblyName) { }
    }
}
