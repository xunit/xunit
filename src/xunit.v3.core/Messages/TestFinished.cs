using System.Collections.Generic;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

public partial class TestFinished
{
	IReadOnlyDictionary<string, TestAttachment>? attachments;

	/// <inheritdoc/>
	public required IReadOnlyDictionary<string, TestAttachment> Attachments
	{
		get => this.ValidateNullablePropertyValue(attachments, nameof(Attachments));
		set => attachments = Guard.ArgumentNotNull(value, nameof(Attachments));
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(attachments, nameof(Attachments), invalidProperties);
	}
}
