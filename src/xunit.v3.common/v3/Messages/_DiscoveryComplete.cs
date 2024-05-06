using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that the discovery process has been completed for
/// the requested assembly.
/// </summary>
[JsonTypeID("discovery-complete")]
public sealed class _DiscoveryComplete : _TestAssemblyMessage
{
	/// <summary>
	/// Gets a count of the number of test cases that passed the filter and will be run.
	/// </summary>
	public int TestCasesToRun { get; set; }

	internal override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		base.Deserialize(root);

		if (TryGetInt(root, nameof(TestCasesToRun)) is int result)
			TestCasesToRun = result;
	}

	internal override void Serialize(JsonObjectSerializer serializer)
	{
		base.Serialize(serializer);

		serializer.Serialize(nameof(TestCasesToRun), TestCasesToRun);
	}
}
