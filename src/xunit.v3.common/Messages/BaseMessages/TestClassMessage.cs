using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Base message interface for all messages related to test classes.
/// </summary>
public abstract class TestClassMessage : TestCollectionMessage
{
	/// <summary>
	/// Gets the test class's unique ID. Can be used to correlate test messages with the appropriate
	/// test class that they're related to.
	/// </summary>
	public required string? TestClassUniqueID { get; set; }

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		TestClassUniqueID = JsonDeserializer.TryGetString(root, nameof(TestClassUniqueID));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(TestClassUniqueID), TestClassUniqueID);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0}({1})", GetType().Name, TestClassUniqueID.Quoted());
}
