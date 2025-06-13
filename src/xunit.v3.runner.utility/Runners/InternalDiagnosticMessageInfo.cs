using System;

namespace Xunit.Runners;

/// <summary>
/// Represents an internal diagnostic message from the xUnit.net system.
/// </summary>
[Obsolete("Please use the MessageInfo class from the Xunit.SimpleRunner namespace. This class will be removed in the next major release.")]
public class InternalDiagnosticMessageInfo(string message)
{
	/// <summary>
	/// The diagnostic message.
	/// </summary>
	public string Message { get; } = message;
}
