using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public partial class TestMethodMessage
{
	/// <inheritdoc/>
	public required string? TestMethodUniqueID { get; set; }

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		TestMethodUniqueID = JsonDeserializer.TryGetString(root, nameof(TestMethodUniqueID));
	}
}
