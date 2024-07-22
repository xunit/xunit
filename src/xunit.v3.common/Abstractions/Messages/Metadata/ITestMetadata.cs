using System.Collections.Generic;

namespace Xunit.Sdk;

/// <summary>
/// Represents metadata about a test.
/// </summary>
public interface ITestMetadata
{
	/// <summary>
	/// Gets the display name of the test.
	/// </summary>
	string TestDisplayName { get; }

	/// <summary>
	/// Gets the trait values associated with this test case. If
	/// there are none, or the framework does not support traits,
	/// this should return an empty dictionary (not <c>null</c>).
	/// </summary>
	IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; }

	/// <summary>
	/// Gets a unique identifier for the test.
	/// </summary>
	/// <remarks>
	/// The unique identifier for a test should be able to discriminate among test, even those which are
	/// varied invocations against the same test method (i.e., theories). This identifier should remain
	/// stable until such time as the developer changes some fundamental part of the identity (assembly,
	/// class name, test name, or test data). Recompilation of the test assembly is reasonable as a
	/// stability changing event.
	/// </remarks>
	string UniqueID { get; }
}
