using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that a line of output was provided for a test.
/// </summary>
public class _TestOutput : _TestMessage
{
	string? output;

	/// <summary>
	/// Gets or sets the line of output.
	/// </summary>
	public string Output
	{
		get => this.ValidateNullablePropertyValue(output, nameof(Output));
		set => output = Guard.ArgumentNotNull(value, nameof(Output));
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidateNullableProperty(output, nameof(Output), invalidProperties);
	}
}
