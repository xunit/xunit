using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Base message for all messages related to test collections.
/// </summary>
public abstract class TestCollectionMessage : TestAssemblyMessage
{
	string? testCollectionUniqueID;

	/// <summary>
	/// Gets the test collection's unique ID. Can be used to correlate test messages with the appropriate
	/// test collection that they're related to.
	/// </summary>
	public required string TestCollectionUniqueID
	{
		get => this.ValidateNullablePropertyValue(testCollectionUniqueID, nameof(TestCollectionUniqueID));
		set => testCollectionUniqueID = Guard.ArgumentNotNullOrEmpty(value, nameof(TestCollectionUniqueID));
	}

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		testCollectionUniqueID = JsonDeserializer.TryGetString(root, nameof(TestCollectionUniqueID));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(TestCollectionUniqueID), TestCollectionUniqueID);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0}({1})", GetType().Name, testCollectionUniqueID.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testCollectionUniqueID, nameof(TestCollectionUniqueID), invalidProperties);
	}
}
