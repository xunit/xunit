using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that a test was not run because it was excluded (either because
/// it was marked as explicit and explicit tests weren't run, or because it was marked as
/// not explicit as only explicit tests were run).
/// </summary>
[JsonTypeID("test-not-run")]
public sealed class TestNotRun : TestResultMessage, IWritableExecutionMetadata
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
