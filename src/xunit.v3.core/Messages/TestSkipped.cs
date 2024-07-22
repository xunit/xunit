using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class TestSkipped
{
	string? reason;

	/// <inheritdoc/>
	public required string Reason
	{
		get => this.ValidateNullablePropertyValue(reason, nameof(Reason));
		set => reason = Guard.ArgumentNotNull(value, nameof(Reason));
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(reason, nameof(Reason), invalidProperties);
	}
}
