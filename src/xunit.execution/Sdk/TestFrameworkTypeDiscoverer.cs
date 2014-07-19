using System;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestFrameworkTypeDiscoverer : ITestFrameworkTypeDiscoverer
    {
        public Type GetTestFrameworkType(IAttributeInfo attribute)
        {
            var args = attribute.GetConstructorArguments().Cast<string>().ToArray();
            return Reflector.GetType(args[1], args[0]);
        }
    }
}
