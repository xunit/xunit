using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test has finished executing.
/// </summary>
[JsonTypeID("test-finished")]
public sealed class _TestFinished : _TestResultMessage, _IWritableExecutionMetadata
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
