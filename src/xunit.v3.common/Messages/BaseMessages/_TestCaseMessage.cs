using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Base message for all messages related to test cases.
/// </summary>
public abstract class _TestCaseMessage : _TestMethodMessage
{
	string? testCaseUniqueID;

	/// <summary>
	/// Gets the test case's unique ID. Can be used to correlate test messages with the appropriate
	/// test case that they're related to.
	/// </summary>
	public string TestCaseUniqueID
	{
		get => this.ValidateNullablePropertyValue(testCaseUniqueID, nameof(TestCaseUniqueID));
		set => testCaseUniqueID = Guard.ArgumentNotNullOrEmpty(value, nameof(TestCaseUniqueID));
	}

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		testCaseUniqueID = JsonDeserializer.TryGetString(root, nameof(TestCaseUniqueID));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(TestCaseUniqueID), TestCaseUniqueID);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0}({1})", GetType().Name, testCaseUniqueID.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testCaseUniqueID, nameof(TestCaseUniqueID), invalidProperties);
	}
}
