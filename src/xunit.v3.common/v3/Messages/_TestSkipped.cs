using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message indicates that a test was skipped.
/// </summary>
public class _TestSkipped : _TestResultMessage
{
	string? reason;

	/// <summary>
	/// The reason given for skipping the test.
	/// </summary>
	public string Reason
	{
		get => this.ValidateNullablePropertyValue(reason, nameof(Reason));
		set => reason = Guard.ArgumentNotNull(value, nameof(Reason));
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidateNullableProperty(reason, nameof(Reason), invalidProperties);
	}
}
