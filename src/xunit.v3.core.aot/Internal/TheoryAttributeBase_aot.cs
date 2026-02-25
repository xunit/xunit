namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public abstract class TheoryAttributeBase : FactAttributeBase
{
	internal TheoryAttributeBase()
	{ }

	/// <summary>
	/// Gets a flag which indicates whether the test method wants to skip enumerating data during
	/// discovery. This will cause the theory to yield a single test case for all data, and the
	/// data discovery will be performed during test execution instead of discovery.
	/// </summary>
	public bool DisableDiscoveryEnumeration { get; set; }

	/// <summary>
	/// Gets a flag which indicates whether the test should be skipped (rather than failed) for
	/// a lack of data.
	/// </summary>
	public bool SkipTestWithoutData { get; set; }
}
