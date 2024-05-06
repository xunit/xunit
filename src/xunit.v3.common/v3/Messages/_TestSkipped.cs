using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that a test was skipped.
/// </summary>
[JsonTypeID("test-skipped")]
public class _TestSkipped : _TestResultMessage
{
	string? reason;

	/// <summary>
	/// The reason given for skipping the test.
	/// </summary>
	public string Reason
	{
		get => this.ValidateNullablePropertyValue(reason, nameof(Reason));
		set => reason = Guard.ArgumentNotNull(value, nameof(Reason));
	}

	internal override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		reason = TryGetString(root, nameof(Reason));
	}

	internal override void Serialize(JsonObjectSerializer serializer)
	{
		base.Serialize(serializer);

		serializer.Serialize(nameof(Reason), Reason);
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(reason, nameof(Reason), invalidProperties);
	}
}
