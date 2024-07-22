using System.Collections.Generic;
using Xunit.Internal;

namespace Xunit.v3;

public partial class InternalDiagnosticMessage
{
	string? message;

	/// <inheritdoc/>
	public required string Message
	{
		get => this.ValidateNullablePropertyValue(message, nameof(Message));
		set => message = Guard.ArgumentNotNull(value, nameof(Message));
	}

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties) =>
		ValidatePropertyIsNotNull(message, nameof(Message), invalidProperties);
}
