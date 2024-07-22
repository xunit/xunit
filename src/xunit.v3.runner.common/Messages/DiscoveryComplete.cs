using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public partial class DiscoveryComplete
{
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

		TestCasesToRun = JsonDeserializer.TryGetInt(root, nameof(TestCasesToRun)) ?? TestCasesToRun;
	}
}
