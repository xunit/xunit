using System;
using Xunit.Abstractions;

#if XUNIT_CORE_DLL
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    /// <summary>
    /// Default implementation of <see cref="IDiagnosticMessage"/>.
    /// </summary>
    public class DiagnosticMessage : LongLivedMarshalByRefObject, IDiagnosticMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticMessage"/> class.
        /// </summary>
        public DiagnosticMessage() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticMessage"/> class.
        /// </summary>
        /// <param name="message">The message to send</param>
        public DiagnosticMessage(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DiagnosticMessage"/> class.
        /// </summary>
        /// <param name="format">The format of the message to send</param>
        /// <param name="args">The arguments used to format the message</param>
        public DiagnosticMessage(string format, params object[] args)
        {
            Message = String.Format(format, args);
        }

        /// <inheritdoc/>
        public string Message { get; set; }
    }
}