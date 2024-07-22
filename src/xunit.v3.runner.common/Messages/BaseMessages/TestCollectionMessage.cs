using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public partial class TestCollectionMessage
{
	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be <see cref="MessageSinkMessage.UnsetStringPropertyValue"/> if there was no value provided during deserialization.
	/// </remarks>
	public required string TestCollectionUniqueID { get; set; } = UnsetStringPropertyValue;

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		TestCollectionUniqueID = JsonDeserializer.TryGetString(root, nameof(TestCollectionUniqueID)) ?? TestCollectionUniqueID;
	}
}
