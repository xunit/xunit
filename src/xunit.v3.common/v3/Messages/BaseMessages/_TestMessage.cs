using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// Base message for all messages related to tests.
/// </summary>
public abstract class _TestMessage : _TestCaseMessage
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

	internal override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		testUniqueID = TryGetString(root, nameof(TestUniqueID));
	}

	internal override void Serialize(JsonObjectSerializer serializer)
	{
		base.Serialize(serializer);

		serializer.Serialize(nameof(TestUniqueID), TestUniqueID);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0}({1})", GetType().Name, testUniqueID.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testUniqueID, nameof(TestUniqueID), invalidProperties);
	}
}
