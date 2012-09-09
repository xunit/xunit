using System;
using Xunit.Sdk;

namespace Xunit
{
    [XunitDiscoverer(DiscovererType = typeof(TheoryDiscoverer))]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TheoryAttribute : Fact2Attribute
    {
    }
}
