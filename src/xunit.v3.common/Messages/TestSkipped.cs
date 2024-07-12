using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test was skipped.
/// </summary>
[JsonTypeID("test-skipped")]
public sealed class TestSkipped : TestResultMessage
{
	string? reason;

	/// <summary>
	/// The reason given for skipping the test.
	/// </summary>
	public required string Reason
	{
		get => this.ValidateNullablePropertyValue(reason, nameof(Reason));
		set => reason = Guard.ArgumentNotNull(value, nameof(Reason));
	}

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		reason = JsonDeserializer.TryGetString(root, nameof(Reason));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

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
