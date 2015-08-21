namespace Xunit.Runners
{
    /// <summary>
    /// Represents a diagnostic message from the xUnit.net system or third party extension.
    /// </summary>
    public class DiagnosticMessageInfo
    {
        /// <summary/>
        public DiagnosticMessageInfo(string message)
        {
            Message = message;
        }

        /// <summary>
        /// The diagnostic message.
        /// </summary>
        public string Message { get; }
    }
}
