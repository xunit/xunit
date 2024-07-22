using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class AfterTestStarting
{
	string? attributeName;

	/// <inheritdoc/>
	public required string AttributeName
	{
		get => this.ValidateNullablePropertyValue(attributeName, nameof(AttributeName));
		set => attributeName = Guard.ArgumentNotNull(value, nameof(AttributeName));
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(attributeName, nameof(AttributeName), invalidProperties);
	}
}
