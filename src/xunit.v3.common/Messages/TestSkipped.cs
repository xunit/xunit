using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test was skipped.
/// </summary>
[JsonTypeID("test-skipped")]
public sealed class TestSkipped : TestResultMessage, IWritableExecutionMetadata
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

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		root.DeserializeExecutionMetadata(this);
		reason = JsonDeserializer.TryGetString(root, nameof(Reason));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.SerializeExecutionMetadata(this);
		serializer.Serialize(nameof(Reason), Reason);
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(reason, nameof(Reason), invalidProperties);
	}
}
