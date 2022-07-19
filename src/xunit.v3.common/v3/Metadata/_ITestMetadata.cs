using System.Collections.Generic;

namespace Xunit.v3;

/// <summary>
/// Represents metadata about a test.
/// </summary>
public interface _ITestMetadata
{
	/// <summary>
	/// Gets a flag indicating whether this test was marked as explicit or not.
	/// </summary>
	bool Explicit { get; }

	/// <summary>
	/// Gets the display name of the test.
	/// </summary>
	string TestDisplayName { get; }

	/// <summary>
	/// A value greater than zero marks the test as having a timeout, and gets or sets the
	/// timeout (in milliseconds).
	/// </summary>
	/// <remarks>
	/// WARNING: Using this with parallelization turned on will result in undefined behavior.
	/// Timeout is only supported when parallelization is disabled, either globally or with
	/// a parallelization-disabled test collection.
	/// </remarks>
	int Timeout { get; }

	/// <summary>
	/// Gets the trait values associated with this test case. If
	/// there are none, or the framework does not support traits,
	/// this should return an empty dictionary (not <c>null</c>).
	/// </summary>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits { get; }
}
