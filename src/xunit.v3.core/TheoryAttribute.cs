#pragma warning disable CA1813 // This attribute is unsealed because it's an extensibility point

using System;
using Xunit.Sdk;

namespace Xunit;

/// <summary>
/// Marks a test method as being a data theory. Data theories are tests which are fed
/// various bits of data from a data source, mapping to parameters on the test method.
/// If the data source contains multiple rows, then the test method is executed
/// multiple times (once with each data row). Data is provided by attributes which
/// derive from <see cref="DataAttribute"/> (notably, <see cref="InlineDataAttribute"/> and
/// <see cref="MemberDataAttribute"/>).
/// </summary>
[XunitTestCaseDiscoverer(typeof(TheoryDiscoverer))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TheoryAttribute : FactAttribute
{
	/// <summary>
	/// Returns <c>true</c> if the data attribute wants to skip enumerating data during discovery.
	/// This will cause the theory to yield a single test case for all data, and the data discovery
	/// will be during test execution instead of discovery.
	/// </summary>
	public bool DisableDiscoveryEnumeration { get; set; }
}
