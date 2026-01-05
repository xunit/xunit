using System;
using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public partial class DiscoveryComplete
{
	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="DateTimeOffset.UtcNow"/> if there was no value provided during deserialization.
	/// </remarks>
	public required DateTimeOffset FinishTime { get; set; }

	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <c>0</c> if there was no value provided during deserialization.
	/// </remarks>
	public required int TestCasesToRun { get; set; }

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		FinishTime = JsonDeserializer.TryGetDateTimeOffset(root, nameof(FinishTime)) ?? DateTimeOffset.UtcNow;
		TestCasesToRun = JsonDeserializer.TryGetInt(root, nameof(TestCasesToRun)) ?? TestCasesToRun;
	}
}
