namespace Xunit.Runners;

/// <summary>
/// Represents an internal diagnostic message from the xUnit.net system.
/// </summary>
public class InternalDiagnosticMessageInfo(string message)
{
	/// <summary>
	/// The diagnostic message.
	/// </summary>
	public string Message { get; } = message;
}
