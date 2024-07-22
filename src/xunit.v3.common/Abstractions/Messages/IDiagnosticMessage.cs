namespace Xunit.Sdk;

/// <summary>
/// This message is sent when code (1st or 3rd party) wants to alert the user to a situation that may require
/// diagnostic investigation. This is typically not displayed unless the user has explicitly asked for diagnostic
/// messages to be displayed (see <see href="https://xunit.net/docs/configuration-files#diagnosticMessages"/> on
/// how to enable display of diagnostic messages).
/// </summary>
public interface IDiagnosticMessage : IMessageSinkMessage
{
	/// <summary>
	/// Gets the diagnostic message.
	/// </summary>
	string Message { get; }
}
