namespace Xunit.Runners;

/// <summary>
/// Represents a diagnostic message from the xUnit.net system or third party extension.
/// </summary>
[Obsolete("Please use the MessageInfo class from the Xunit.SimpleRunner namespace. This class will be removed in the next major release.")]
public class DiagnosticMessageInfo(string message)
{
	/// <summary>
	/// The diagnostic message.
	/// </summary>
	public string Message { get; } = message;
}
