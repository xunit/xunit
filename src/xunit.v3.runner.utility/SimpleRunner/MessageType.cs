namespace Xunit.SimpleRunner;

/// <summary>
/// Represents the type of message that was sent.
/// </summary>
public enum MessageType
{
	/// <summary>
	/// Indicates that the message was a diagnostic message.
	/// </summary>
	DiagnosticMessage = 1,

	/// <summary>
	/// Indicates that the message was an internal diagnostic message.
	/// </summary>
	InternalDiagnosticMessage = 2,
}
