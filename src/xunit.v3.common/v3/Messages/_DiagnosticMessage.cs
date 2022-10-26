using System;
using Xunit.Internal;

namespace Xunit.v3;

/// <summary>
/// This message is sent when code (1st or 3rd party) wants to alert the user to a situation that may require
/// diagnostic investigation. This is typically not displayed unless the user has explicitly asked for diagnostic
/// messages to be displayed (see <see href="https://xunit.net/docs/configuration-files#diagnosticMessages"/> on
/// how to enable display of diagnostic messages).
/// </summary>
public class _DiagnosticMessage : _MessageSinkMessage
{
	string? message;

	/// <summary>
	/// Gets or sets the diagnostic message.
	/// </summary>
	public string Message
	{
		get => message ?? throw new InvalidOperationException($"Attempted to get {nameof(Message)} on an uninitialized '{GetType().FullName}' object");
		set => message = Guard.ArgumentNotNull(value, nameof(Message));
	}

	/// <inheritdoc/>
	public override string ToString() =>
		$"{GetType().Name} message={message.Quoted()}";
}
