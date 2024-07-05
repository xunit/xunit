using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Base message for all messages related to test methods.
/// </summary>
public abstract class _TestMethodMessage : _TestClassMessage
{
	/// <summary>
	/// Gets the test method's unique ID. Can be used to correlate test messages with the appropriate
	/// test method that they're related to.
	/// </summary>
	public string? TestMethodUniqueID { get; set; }

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		TestMethodUniqueID = JsonDeserializer.TryGetString(root, nameof(TestMethodUniqueID));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(TestMethodUniqueID), TestMethodUniqueID);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0}({1})", GetType().Name, TestMethodUniqueID.Quoted());
}
