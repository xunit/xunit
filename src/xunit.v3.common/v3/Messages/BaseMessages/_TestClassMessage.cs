using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// Base message interface for all messages related to test classes.
/// </summary>
public class _TestClassMessage : _TestCollectionMessage
{
	/// <summary>
	/// Gets the test class's unique ID. Can be used to correlate test messages with the appropriate
	/// test class that they're related to. Test class metadata (as <see cref="_ITestClassMetadata"/>)
	/// is provided via <see cref="_TestClassStarting"/> (during execution) and should be cached as needed.
	/// Might be <c>null</c> if the test does not belong to a test class.
	/// </summary>
	public string? TestClassUniqueID { get; set; }

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0}({1})", GetType().Name, TestClassUniqueID.Quoted());
}
