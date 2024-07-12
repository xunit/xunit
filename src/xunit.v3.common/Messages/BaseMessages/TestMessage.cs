using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Base message for all messages related to tests.
/// </summary>
public abstract class TestMessage : TestCaseMessage
{
	string? testUniqueID;

	/// <summary>
	/// Gets the test's unique ID. Can be used to correlate test messages with the appropriate
	/// test that they're related to.
	/// </summary>
	public required string TestUniqueID
	{
		get => this.ValidateNullablePropertyValue(testUniqueID, nameof(TestUniqueID));
		set => testUniqueID = Guard.ArgumentNotNullOrEmpty(value, nameof(TestUniqueID));
	}

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		testUniqueID = JsonDeserializer.TryGetString(root, nameof(TestUniqueID));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

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
