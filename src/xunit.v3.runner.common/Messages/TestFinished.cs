using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

public partial class TestFinished
{
	/// <inheritdoc/>
	/// <remarks>
	/// Note: Will be an empty dictionary if there was no value provided during deserialization.
	/// </remarks>
	public required IReadOnlyDictionary<string, TestAttachment> Attachments { get; set; } = EmptyAttachments;

	/// <inheritdoc/>
	protected override void Deserialize(IReadOnlyDictionary<string, object?> root)
	{
		Guard.ArgumentNotNull(root);

		base.Deserialize(root);

		var attachments = new Dictionary<string, TestAttachment>();
		var attachmentsObj = JsonDeserializer.TryGetObject(root, nameof(Attachments));
		if (attachmentsObj is not null)
			foreach (var kvp in attachmentsObj)
				if (kvp.Value is string stringValue)
					attachments[kvp.Key] = TestAttachment.Parse(stringValue);

		Attachments = attachments;
	}
}
