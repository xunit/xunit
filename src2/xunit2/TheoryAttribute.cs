using System;
using Xunit.Sdk;

namespace Xunit
{
    [XunitDiscoverer("Xunit.Sdk.TheoryDiscoverer", "xunit2")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TheoryAttribute : FactAttribute
    {
    }
}
