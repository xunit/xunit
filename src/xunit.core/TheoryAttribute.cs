using System;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Marks a test method as being a data theory. Data theories are tests which are fed
    /// various bits of data from a data source, mapping to parameters on the test method.
    /// If the data source contains multiple rows, then the test method is executed
    /// multiple times (once with each data row). Data is provided by attributes which
    /// derive from <see cref="DataAttribute"/> (notably, <see cref="InlineDataAttribute"/> and
    /// <see cref="MemberDataAttribute"/>).
    /// </summary>
    [XunitTestCaseDiscoverer("Xunit.Sdk.TheoryDiscoverer", "xunit.execution.{Platform}")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TheoryAttribute : FactAttribute { }
}
