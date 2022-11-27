using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// Base message for all messages related to tests.
/// </summary>
public class _TestMessage : _TestCaseMessage
{
	string? testUniqueID;

	/// <summary>
	/// Gets the test's unique ID. Can be used to correlate test messages with the appropriate
	/// test that they're related to. Test metadata is provided as <see cref="_ITestMetadata"/>
	/// via <see cref="_TestStarting"/> (during execution) and should be cached if required.
	/// </summary>
	public string TestUniqueID
	{
		get => this.ValidateNullablePropertyValue(testUniqueID, nameof(TestUniqueID));
		set => testUniqueID = Guard.ArgumentNotNullOrEmpty(value, nameof(TestUniqueID));
	}

	/// <inheritdoc/>
	public override string ToString() =>
		$"{GetType().Name}({testUniqueID.Quoted()})";
}
