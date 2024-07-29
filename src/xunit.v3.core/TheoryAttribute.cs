#pragma warning disable CA1813 // This attribute is unsealed because it's an extensibility point

using System;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Marks a test method as being a data theory. Data theories are tests which are fed
/// various bits of data from a data source, mapping to parameters on the test method.
/// If the data source contains multiple rows, then the test method is executed
/// multiple times (once with each data row). Data is provided by attributes which
/// implement <see cref="IDataAttribute"/> (most commonly, <see cref="InlineDataAttribute"/>
/// and <see cref="MemberDataAttribute"/>).
/// </summary>
[XunitTestCaseDiscoverer(typeof(TheoryDiscoverer))]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class TheoryAttribute : FactAttribute, ITheoryAttribute
{
	/// <inheritdoc/>
	public bool DisableDiscoveryEnumeration { get; set; }

	/// <inheritdoc/>
	public bool SkipTestWithoutData { get; set; }
}
