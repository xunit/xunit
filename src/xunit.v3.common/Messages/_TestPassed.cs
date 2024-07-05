using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Indicates that a test has passed.
/// </summary>
[JsonTypeID("test-passed")]
public sealed class _TestPassed : _TestResultMessage, _IWritableExecutionMetadata
{
	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		root.DeserializeExecutionMetadata(this);
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.SerializeExecutionMetadata(this);
	}
}
