using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class TestOutput
{
	string? output;

	/// <inheritdoc/>
	public required string Output
	{
		get => this.ValidateNullablePropertyValue(output, nameof(Output));
		set => output = Guard.ArgumentNotNull(value, nameof(Output));
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidatePropertyIsNotNull(output, nameof(Output), invalidProperties);
	}
}
