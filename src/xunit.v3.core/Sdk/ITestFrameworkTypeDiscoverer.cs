using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Interface to be implemented by classes which are used to discover the test framework.
    /// </summary>
    public interface ITestFrameworkTypeDiscoverer
    {
        /// <summary>
        /// Gets the type that implements <see cref="ITestFramework"/> to be used to discover
        /// and run tests.
        /// </summary>
        /// <param name="attribute">The test framework attribute that decorated the assembly</param>
        /// <returns>The test framework type</returns>
        Type GetTestFrameworkType(IAttributeInfo attribute);
    }
}
