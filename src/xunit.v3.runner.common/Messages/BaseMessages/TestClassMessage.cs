using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public partial class TestClassMessage
{
	/// <inheritdoc/>
	public required string? TestClassUniqueID { get; set; }

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		TestClassUniqueID = JsonDeserializer.TryGetString(root, nameof(TestClassUniqueID));
	}
}
