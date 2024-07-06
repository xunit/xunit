using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// This message indicates that the discovery process has been completed for
/// the requested assembly.
/// </summary>
[JsonTypeID("discovery-complete")]
public sealed class DiscoveryComplete : TestAssemblyMessage
{
	/// <summary>
	/// Gets a count of the number of test cases that passed the filter and will be run.
	/// </summary>
	public int TestCasesToRun { get; set; }

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		if (JsonDeserializer.TryGetInt(root, nameof(TestCasesToRun)) is int result)
			TestCasesToRun = result;
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(TestCasesToRun), TestCasesToRun);
	}
}
