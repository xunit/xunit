using System.Collections.Generic;
using System.Globalization;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message is sent when xUnit.net itself wants to log internal messages and state that are typically only
/// used by the xUnit.net team to gain a deeper understanding of potential end user issues. These messages are
/// rarely useful to end users directly, and may result in very noisy logs. This is typically not displayed
/// unless the user has explicit asked for internal diagnostic messages to be displayed (see
/// <a href="https://xunit.net/docs/configuration-files#internalDiagnosticMessages"/> on how to enable
/// display of internal diagnostic messages).
/// </summary>
public class _InternalDiagnosticMessage : _MessageSinkMessage
{
	string? message;

	/// <summary>
	/// Gets or sets the diagnostic message.
	/// </summary>
	public string Message
	{
		get => this.ValidateNullablePropertyValue(message, nameof(Message));
		set => message = Guard.ArgumentNotNull(value, nameof(Message));
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} message={1}", GetType().Name, message.Quoted());

	/// <inheritdoc/>
	protected override void ValidateObjectState(HashSet<string> invalidProperties)
	{
		base.ValidateObjectState(invalidProperties);

		ValidateNullableProperty(message, nameof(Message), invalidProperties);
	}
}
