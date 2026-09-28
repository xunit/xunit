namespace Xunit.SimpleRunner;

/// <summary>
/// Represents either a diagnostic or internal diagnostic message.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Diagnostic messages may come from xUnit.net or from third party extensions.</item>
/// <item>Internal diagnostic messages only come from xUnit.net itself.</item>
/// </list>
/// </remarks>
public class MessageInfo
{
	/// <summary>
	/// Gets the message text of the message.
	/// </summary>
	public required string Message { get; set; }

	/// <summary>
	/// Gets the type of message that was sent.
	/// </summary>
	public required MessageType MessageType { get; set; }
}
