namespace Xunit.Sdk;

/// <summary>
/// This message is sent when xUnit.net itself wants to log internal messages and state that are typically only
/// used by the xUnit.net team to gain a deeper understanding of potential end user issues. These messages are
/// rarely useful to end users directly, and may result in very noisy logs. This is typically not displayed
/// unless the user has explicit asked for internal diagnostic messages to be displayed (see
/// <a href="https://xunit.net/docs/configuration-files#internalDiagnosticMessages"/> on how to enable
/// display of internal diagnostic messages).
/// </summary>
public interface IInternalDiagnosticMessage : IMessageSinkMessage
{
	/// <summary>
	/// Gets the internal diagnostic message.
	/// </summary>
	string Message { get; }
}
