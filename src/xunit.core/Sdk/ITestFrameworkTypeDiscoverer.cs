using System;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public interface ITestFrameworkTypeDiscoverer
    {
        Type GetTestFrameworkType(IAttributeInfo attribute);
    }
}
