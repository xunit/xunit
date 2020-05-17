using System;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestFrameworkTypeDiscoverer"/> that supports attributes
    /// of type <see cref="TestFrameworkDiscovererAttribute"/>.
    /// </summary>
    public class TestFrameworkTypeDiscoverer : ITestFrameworkTypeDiscoverer
    {
        /// <inheritdoc/>
        public Type GetTestFrameworkType(IAttributeInfo attribute)
        {
            var args = attribute.GetConstructorArguments().Cast<string>().ToArray();
            return SerializationHelper.GetType(args[1], args[0]);
        }
    }
}
