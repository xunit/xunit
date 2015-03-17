namespace Xunit.Abstractions
{
    /// <summary>
    /// This message is sent when the test framework wants to report a diagnostic message
    /// to the end user.
    /// </summary>
    public interface IDiagnosticMessage : IMessageSinkMessage
    {
        /// <summary>
        /// Gets the diagnostic message.
        /// </summary>
        string Message { get; }
    }
}