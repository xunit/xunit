namespace Xunit.v3;

/// <summary>
/// Marks a test method as being a data theory. Data theories are tests which are fed
/// various bits of data from a data source, mapping to parameters on the test method.
/// If the data source contains multiple rows, then the test method is executed
/// multiple times (once with each data row). Data is provided by attributes which
/// implement <see cref="IDataAttribute"/> (most commonly, <see cref="InlineDataAttribute"/>
/// and <see cref="MemberDataAttribute"/>). Implementations must be decorated by
/// <see cref="XunitTestCaseDiscovererAttribute"/> to indicate which class is responsible
/// for converting the test method into one or more tests.
/// </summary>
/// <remarks>The attribute can only be applied to methods, and only one attribute is allowed.</remarks>
public interface ITheoryAttribute : IFactAttribute
{
	/// <summary>
	/// Gets a flag which indicates whether the test method wants to skip enumerating data during
	/// discovery. This will cause the theory to yield a single test case for all data, and the
	/// data discovery will be performed during test execution instead of discovery.
	/// </summary>
	bool DisableDiscoveryEnumeration { get; }

	/// <summary>
	/// Gets a flag which indicates whether the test should be skipped (rather than failed) for
	/// a lack of data.
	/// </summary>
	bool SkipTestWithoutData { get; }
}
