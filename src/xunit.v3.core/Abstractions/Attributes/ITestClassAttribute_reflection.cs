using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Optional attribute that is applied to a test class.
/// </summary>
/// <remarks>
/// This attribute may only be applied to test classes, and may only be applied once per test class.
/// </remarks>
public interface ITestClassAttribute
{
	/// <summary>
	/// Determines whether tests in this class runs in parallel with any other tests.
	/// </summary>
	/// <remarks>
	/// This value is only used when the test project <see cref="ParallelMode"/> is <see cref="ParallelMode.All"/>.
	/// </remarks>
	bool DisableParallelization { get; }
}
