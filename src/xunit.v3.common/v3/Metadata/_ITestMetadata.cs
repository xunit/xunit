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
	/// Gets the trait values associated with this test case. If
	/// there are none, or the framework does not support traits,
	/// this should return an empty dictionary (not <c>null</c>).
	/// </summary>
	public IReadOnlyDictionary<string, IReadOnlyList<string>> Traits { get; }
}
