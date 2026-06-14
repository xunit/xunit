using Xunit.Sdk;

namespace Xunit;

partial class TestClassAttribute
{
	/// <summary>
	/// Determines whether tests in this class runs in parallel with any other tests.
	/// </summary>
	/// <remarks>
	/// This value is only used when the test project <see cref="ParallelMode"/> is <see cref="ParallelMode.All"/>.
	/// </remarks>
	public bool DisableParallelization { get; set; }
}
