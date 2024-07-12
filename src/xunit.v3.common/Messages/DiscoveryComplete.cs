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
	int? testCasesToRun;

	/// <summary>
	/// Gets a count of the number of test cases that passed the filter and will be run.
	/// </summary>
	public required int TestCasesToRun
	{
		get => this.ValidateNullablePropertyValue(testCasesToRun, nameof(TestCasesToRun));
		set => testCasesToRun = value;
	}

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		testCasesToRun = JsonDeserializer.TryGetInt(root, nameof(TestCasesToRun));
	}

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(TestCasesToRun), TestCasesToRun);
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(testCasesToRun, nameof(TestCasesToRun), invalidProperties);
	}
}
