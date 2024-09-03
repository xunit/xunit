namespace Xunit.Runners;

/// <summary>
/// Represents a diagnostic message from the xUnit.net system or third party extension.
/// </summary>
public class DiagnosticMessageInfo(string message)
{
	/// <summary>
	/// The diagnostic message.
	/// </summary>
	public string Message { get; } = message;
}
